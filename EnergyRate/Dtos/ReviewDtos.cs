namespace EnergyRate.Dtos
{
    public record ReviewResponse(
        int Id,
        int DrinkId,
        int UserId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        Dictionary<string, string> Links
        );

    public record ReviewCreateRequest(
        int UserId,
        int Rating,
        string? Comment
        );

    public record ReviewUpdateRequest(
        int Rating,
        string? Comment
        );

    public class  ReviewQuery : PaginationQuery
    {
        public int? Rating { get; set; }
        public int? UserId { get; set; }
    }
}
