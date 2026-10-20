namespace EnergyRate.Models
{
    public class Drink
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Flavor { get; set; } = string.Empty;
        public decimal CaffeineMgPer100ml { get; set; }
        public int VolumeMl { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Review> Reviews { get; set; } = new();
    }
}
