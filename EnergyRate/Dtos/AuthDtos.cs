using System.ComponentModel.DataAnnotations;

namespace EnergyRate.Dtos
{
    public record RegisterRequest(
        [Required]
        [EmailAddress]
        string Email,

        [Required]
        [MinLength(8)]
        string Password
        );

    public record LoginRequest(
        [Required]
        [EmailAddress]
        string Email,

        [Required]
        string Password
        );

    public record RefreshRequest(
        [Required]
        string RefreshToken
        );

    public record AuthResponse(
        int UserId,
        string Email,
        string Role,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt
        );
}
