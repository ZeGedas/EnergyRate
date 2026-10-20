namespace EnergyRate.Dtos
{
    public record BrandResponse(
        int Id,
        string Name,
        string Country,
        string? Description,
        DateTime CreatedAt,
        Dictionary<string, string> Links
    );

    public record BrandCreateRequest(string Name, string Country, string? Description);
    public record BrandUpdateRequest(string Name, string Country, string? Description);

    public class BrandQuery : PaginationQuery
    {
        public string? Country { get; set; }
    }
}
