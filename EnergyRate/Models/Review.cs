namespace EnergyRate.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int DrinkId { get; set; }
        public Drink? Drink { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int Rating { get; set; } // Rating from 1 to 5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
