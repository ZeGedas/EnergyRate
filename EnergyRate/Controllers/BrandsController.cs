using EnergyRate.Data;
using EnergyRate.Dtos;
using EnergyRate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace EnergyRate.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BrandsController(AppDbContext db) => _db = db;

        private BrandResponse ToResponse(Brand brand) => new(
            brand.Id, brand.Name, brand.Country, brand.Description, brand.CreatedAt,
            new Dictionary<string, string>
            {
                ["self"] = Url.Action(nameof(GetOne), new { brandId = brand.Id })!,
                ["drinks"] = Url.Action("GetList", "Drinks", new { brandId = brand.Id }) ?? ""
            });

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BrandResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<PagedResult<BrandResponse>>> GetList([FromQuery] BrandQuery query)
        {
            var brandsQuery = _db.Brands.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Country))
            {
                brandsQuery = brandsQuery.Where(brand => brand.Country.ToLower() == query.Country.ToLower());
            }
            
            var total = await brandsQuery.CountAsync();
            var items = await brandsQuery.OrderBy(brand => brand.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return Ok(new PagedResult<BrandResponse>
            {
                Items = items.Select(ToResponse).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            });
        }

        [HttpGet("{brandId:int}", Name = "GetBrandById")]
        [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BrandResponse>> GetOne (int brandId)
        {
            var brand = await _db.Brands.FindAsync(brandId);
            if (brand == null) return NotFound( new {message = $"Brand with ID {brandId} not found."});
            return Ok(ToResponse(brand));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BrandResponse>> Create([FromBody] BrandCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Country))
            {
                return UnprocessableEntity(new { message = "Name and Country are required fields." });
            }
            if (await _db.Brands.AnyAsync(b => b.Name.ToLower() == request.Name.ToLower()))
            {
                return Conflict(new { message = $"Brand with name '{request.Name}' already exists." });
            }

            var brand = new Brand
            {
                Name = request.Name,
                Country = request.Country,
                Description = request.Description,
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { brandId = brand.Id }, ToResponse(brand));
        }

        [HttpPut("{brandId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int brandId, [FromBody] BrandUpdateRequest request)
        {
            var brand = await _db.Brands.FindAsync(brandId);
            if (brand == null) return NotFound(new { message = $"Brand with ID {brandId} not found." });
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Country))
            {
                return UnprocessableEntity(new { message = "Name and Country are required fields." });
            }
            if (await _db.Brands.AnyAsync(b => b.Id != brandId && b.Name.ToLower() == request.Name.ToLower()))
            {
                return Conflict(new { message = $"Brand with name '{request.Name}' already exists." });
            }
            brand.Name = request.Name;
            brand.Country = request.Country;
            brand.Description = request.Description;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{brandId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int brandId)
        {
            var brand = await _db.Brands.FindAsync(brandId);
            if (brand is null) return NotFound(new { message = $"Brand with ID {brandId} not found." });
            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
