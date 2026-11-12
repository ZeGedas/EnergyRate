namespace EnergyRate.Models
{
    public enum UserRole
    {
        Member = 0,
        Admin = 1,
        Guest = 2
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Member;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public int TokenVersion { get; set; } = 0;

    }
}
