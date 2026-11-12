using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnergyRate.Models;

namespace EnergyRate.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured.");

            var issuer = _configuration["Jwt:Issuer"] ?? "EnergyRate";
            var audience = _configuration["Jwt:Audience"] ?? "EnergyRateClient";

            var expirationMinutes = _configuration.GetValue<int>
                ("Jwt:AccessTokenExpirationMinutes", 15);

            var securityKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials
                (securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("TokenVersion", user.TokenVersion.ToString())
            };

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(expirationMinutes);
            
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiresAt);
        }
    }
}
