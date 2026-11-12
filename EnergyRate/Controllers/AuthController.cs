using System.Security.Cryptography;
using System.Text;
using EnergyRate.Data;
using EnergyRate.Dtos;
using EnergyRate.Models;
using EnergyRate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyRate.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokenService;

        public AuthController(
            AppDbContext db,
            TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _db.Users
                .AnyAsync(u => u.Email == email);

            if (existingUser)
            {
                return Conflict(new { message = "Email is already registered." });
            }

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Member
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (accessToken, accessExpiresAt) = _tokenService.GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshExpiresAt = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = HashToken(refreshToken);
            user.RefreshTokenExpiresAt = refreshExpiresAt;

            await _db.SaveChangesAsync();

            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.Role.ToString(),
                accessToken,
                accessExpiresAt,
                refreshToken,
                refreshExpiresAt
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var (accessToke, accessExpiresAt) = _tokenService.GenerateAccessToken(user);

            var refreshToken = GenerateRefreshToken();
            var refreshExpiresAt = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = HashToken(refreshToken);
            user.RefreshTokenExpiresAt = refreshExpiresAt;

            await _db.SaveChangesAsync();

            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.Role.ToString(),
                accessToke,
                accessExpiresAt,
                refreshToken,
                refreshExpiresAt
            );

            return Ok(response);
        }

        // POST: api/auth/refresh
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var hashedToken = HashToken(request.RefreshToken);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedToken);

            if (user == null || user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var (accessToken, accessExpiresAt) = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshExpiresAt = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = HashToken(newRefreshToken);
            user.RefreshTokenExpiresAt = newRefreshExpiresAt;

            await _db.SaveChangesAsync();

            var response = new AuthResponse(
                user.Id,
                user.Email,
                user.Role.ToString(),
                accessToken,
                accessExpiresAt,
                newRefreshToken,
                newRefreshExpiresAt
            );

            return Ok(response);
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            var hashedToken = HashToken(request.RefreshToken);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedToken);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            user.TokenVersion++;

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }
    }
}
