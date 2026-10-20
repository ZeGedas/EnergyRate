namespace EnergyRate.Dtos
{

    public record DrinkResponse(
        int Id,
        int BrandId,
        string Name,
        string Flavor,
        decimal CaffeineMgPer100ml,
        int VolumeMl,
        decimal Price,
        DateTime CreatedAt,
        double? AverageRating,
        int ReviewCount,
        Dictionary<string, string> Links
    );

    public record DrinkCreateRequest(
        string Name,
        string Flavor,
        decimal CaffeineMgPer100ml,
        int VolumeMl,
        decimal Price
    );

    public record DrinkUpdateRequest(
        string Name,
        string Flavor,
        decimal CaffeineMgPer100ml,
        int VolumeMl,
        decimal Price
    );

    public class DrinkQuery : PaginationQuery
    {
        public string? Flavor { get; set; }
        public decimal? MinCaffeine { get; set; }
    }
}
