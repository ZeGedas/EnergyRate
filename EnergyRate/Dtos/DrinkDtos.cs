namespace EnergyRate.Dtos
{

    public record DrinkResponse(
        int Id,
        int BrandId,
        string Name,
        string Flavor,
        decimal CaffeineMgPer100ml,
        double VolumeMl,
        decimal Price,
        DateTime CreatedAt,
        Dictionary<string, string> Links
    );

    public record DrinkCreateRequest(
        string Name,
        string Flavor,
        decimal CaffeineMgPer100ml,
        double VolumeMl,
        decimal Price
    );

    public record DrinkUpdateRequest(
        string Name,
        string Flavor,
        decimal CaffeineMgPer100ml,
        double VolumeMl,
        decimal Price
    );

    public class DrinkQuery : PaginationQuery
    {
        public string? Flavor { get; set; }
        public decimal? MinCaffeine { get; set; }
    }
}
