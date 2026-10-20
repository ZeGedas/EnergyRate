using EnergyRate.Data;
using EnergyRate.Dtos;
using EnergyRate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyRate.Controllers
{
    [ApiController]
    [Route("api/brands/{brandId:int}/drinks/{drinkId:int}/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReviewsController(AppDbContext db) => _db = db;
        private ReviewResponse ToResponse(Review review, int brandId) => new(
            review.Id, review.DrinkId, review.UserId, review.Rating, review.Comment, review.CreatedAt,
            new Dictionary<string, string>
            {
                ["self"] = Url.Action(nameof(GetOne), new { brandId, drinkId = review.DrinkId, reviewId = review.Id })!,
                ["drink"] = Url.Action("GetOne", "Drinks", new { brandId, drinkId = review.DrinkId })!,
                ["brand"] = Url.Action("GetOne", "Brands", new { brandId })!
            });
        private async Task<bool> DrinkExists(int brandId, int drinkId) =>
            await _db.Drinks.AnyAsync(d => d.Id == drinkId && d.BrandId == brandId);

        [HttpGet]
        public async Task<ActionResult<PagedResult<ReviewResponse>>> GetList(int brandId, int drinkId, [FromQuery] ReviewQuery query)
        {
            if (!await DrinkExists(brandId, drinkId))
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found for Brand ID {brandId}" });
            }

            var reviewsQuery = _db.Reviews
                .Include(r => r.Drink)
                .Where(r => r.DrinkId == drinkId);

            if (query.Rating.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Rating == query.Rating.Value);
            }
            if (query.UserId.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.UserId == query.UserId.Value);
            }

            var total = await reviewsQuery.CountAsync();

            var items = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return Ok(new PagedResult<ReviewResponse>
            {
                Items = items.Select(r => ToResponse(r, brandId)).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            });
        }

        [HttpGet("{reviewId:int}", Name = "GetReviewById")]
        [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReviewResponse>> GetOne(int brandId, int drinkId, int reviewId)
        {
            if (!await DrinkExists(brandId, drinkId))
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found for Brand ID {brandId}" });
            }

            var review = await _db.Reviews
                .Include(r => r.Drink)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.DrinkId == drinkId);

            if (review == null)
            {
                return NotFound(new { message = $"Review with ID {reviewId} not found for Drink ID {drinkId}" });
            }
            return Ok(ToResponse(review, brandId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ReviewResponse>> Create(int brandId, int drinkId, [FromBody] ReviewCreateRequest request)
        {
            if (!await DrinkExists(brandId, drinkId))
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found for Brand ID {brandId}" });
            }
            if (request.Rating < 1 || request.Rating > 5)
            {
                return UnprocessableEntity(new { message = "Rating must be between 1 and 5." });
            }
            if (!await _db.Users.AnyAsync(u => u.Id == request.UserId))
            {
                return NotFound(new { message = $"User with ID {request.UserId} not found." });
            }

            var alreadyReviewed = await _db.Reviews.AnyAsync(r => r.DrinkId == drinkId && r.UserId == request.UserId);
            if (alreadyReviewed)
            {
                return Conflict(new { message = $"User with ID {request.UserId} has already reviewed Drink ID {drinkId}." });
            }

            var review = new Review
            {
                DrinkId = drinkId,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
            };
            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { brandId, drinkId, reviewId = review.Id }, ToResponse(review, brandId));
        }

        [HttpPut("{reviewId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ReviewResponse>> Update(int brandId, int drinkId, int reviewId, [FromBody] ReviewUpdateRequest request)
        {
            if (!await DrinkExists(brandId, drinkId))
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found for Brand ID {brandId}" });
            }

            var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.DrinkId == drinkId);
            if (review == null)
            {
                return NotFound(new { message = $"Review with ID {reviewId} not found for Drink ID {drinkId}" });
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return UnprocessableEntity(new { message = "Rating must be between 1 and 5." });
            }

            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{reviewId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int brandId, int drinkId, int reviewId)
        {
            if (!await DrinkExists(brandId, drinkId))
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found for Brand ID {brandId}" });
            }
            var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.DrinkId == drinkId);
            if (review is null)
            {
                return NotFound(new { message = $"Review with ID {reviewId} not found for Drink ID {drinkId}" });
            }
            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
