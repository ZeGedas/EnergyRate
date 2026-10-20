using EnergyRate.Data;
using EnergyRate.Dtos;
using EnergyRate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyRate.Controllers
{

    [ApiController]
    [Route("api/brands/{brandId:int}/drinks")]
    public class DrinksController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DrinksController(AppDbContext db) => _db = db;

        private DrinkResponse ToResponse(Drink drink) => new(
            drink.Id, drink.BrandId, drink.Name, drink.Flavor, drink.CaffeineMgPer100ml,
            drink.VolumeMl, drink.Price, drink.CreatedAt,
            new Dictionary<string, string>
            {
                ["self"] = Url.Action(nameof(GetOne), new { brandId = drink.BrandId, drinkId = drink.Id })!,
                ["brand"] = Url.Action("GetOne", "Brands", new { brandId = drink.BrandId })!,
                ["reviews"] = Url.Action("GetList", "Reviews", new { brandId = drink.BrandId, drinkId = drink.Id }) ?? ""
            });

        private async Task<bool> BrandExists(int brandId) => await _db.Brands.AnyAsync(b => b.Id == brandId);

        [HttpGet]
        public async Task<ActionResult<PagedResult<DrinkResponse>>> GetList(int brandId, [FromQuery] DrinkQuery query)
        {
            if (!await BrandExists(brandId))
            {
                return NotFound(new { message = $"Brand with ID {brandId} not found" });
            }

            var drinksQuery = _db.Drinks.Where(d => d.BrandId == brandId);

            if (!string.IsNullOrWhiteSpace(query.Flavor))
            {
                drinksQuery = drinksQuery.Where(d => d.Flavor.ToLower() == query.Flavor.ToLower());
            }

            if (query.MinCaffeine.HasValue)
            {
                drinksQuery = drinksQuery.Where(d => d.CaffeineMgPer100ml >= query.MinCaffeine.Value);
            }

            var total = await drinksQuery.CountAsync();
            var items = await drinksQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return Ok(new PagedResult<DrinkResponse>
            {
                Items = items.Select(ToResponse).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            });
        }

        [HttpGet("{drinkId:int}", Name = "GetDrinkById")]
        public async Task<ActionResult<DrinkResponse>> GetOne(int brandId, int drinkId)
        {
            if (!await BrandExists(brandId))
            {
                return NotFound(new { message = $"Brand with ID {brandId} not found" });
            }

            var drink = await _db.Drinks.FirstOrDefaultAsync(d => d.Id == drinkId && d.BrandId == brandId);
            if (drink is null)
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found" });
            }

            return Ok(ToResponse(drink));
        }

        [HttpPost]
        public async Task<ActionResult<DrinkResponse>> Create(int brandId, [FromBody] DrinkCreateRequest request)
        {
            if (!await BrandExists(brandId))
            {
                return NotFound(new { message = $"Brand with ID {brandId} not found" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return UnprocessableEntity(new { message = "Name is required" });
            }
            if (string.IsNullOrWhiteSpace(request.Flavor))
            {
                return UnprocessableEntity(new { message = "Flavor is required" });
            }
            if (request.CaffeineMgPer100ml < 0)
            {
                return UnprocessableEntity(new { message = "CaffeineMgPer100ml must be non-negative" });
            }
            if (request.VolumeMl <= 0)
            {
                return UnprocessableEntity(new { message = "VolumeMl must be positive" });
            }
            if (request.Price < 0)
            {
                return UnprocessableEntity(new { message = "Price must be non-negative" });
            }

            var alreadyExistsDrink = await _db.Drinks.AnyAsync(d => d.BrandId == brandId && d.Name.ToLower() == request.Name.ToLower());
            if (alreadyExistsDrink)
            {
                return Conflict(new { message = $"Drink with name '{request.Name}' already exists for Brand ID {brandId}." });
            }

            var drink = new Drink
            {
                BrandId = brandId,
                Name = request.Name,
                Flavor = request.Flavor,
                CaffeineMgPer100ml = request.CaffeineMgPer100ml,
                VolumeMl = (int)request.VolumeMl,
                Price = request.Price
            };
            _db.Drinks.Add(drink);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { brandId, drinkId = drink.Id }, ToResponse(drink));
        }

        [HttpPut("{drinkId:int}")]
        public async Task<IActionResult> Update(int brandId, int drinkId, [FromBody] DrinkUpdateRequest request)
        {
            if (!await BrandExists(brandId))
            {
                return NotFound(new { message = $"Brand with ID {brandId} not found" });
            }
            var drink = await _db.Drinks.FirstOrDefaultAsync(d => d.Id == drinkId && d.BrandId == brandId);
            if (drink is null)
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found" });
            }
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return UnprocessableEntity(new { message = "Name is required" });
            }
            if (string.IsNullOrWhiteSpace(request.Flavor))
            {
                return UnprocessableEntity(new { message = "Flavor is required" });
            }
            if (request.CaffeineMgPer100ml < 0)
            {
                return UnprocessableEntity(new { message = "CaffeineMgPer100ml must be non-negative" });
            }
            if (request.VolumeMl <= 0)
            {
                return UnprocessableEntity(new { message = "VolumeMl must be positive" });
            }
            if (request.Price < 0)
            {
                return UnprocessableEntity(new { message = "Price must be non-negative" });
            }
            drink.Name = request.Name;
            drink.Flavor = request.Flavor;
            drink.CaffeineMgPer100ml = request.CaffeineMgPer100ml;
            drink.VolumeMl = (int)request.VolumeMl;
            drink.Price = request.Price;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{drinkId:int}")]
        public async Task<IActionResult> Delete(int brandId, int drinkId)
        {
            if (!await BrandExists(brandId))
            {
                return NotFound(new { message = $"Brand with ID {brandId} not found" });
            }
            var drink = await _db.Drinks.FirstOrDefaultAsync(d => d.Id == drinkId && d.BrandId == brandId);
            if (drink is null)
            {
                return NotFound(new { message = $"Drink with ID {drinkId} not found" });
            }
            _db.Drinks.Remove(drink);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
