using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService(ApplicationDbContext db, IConfiguration config) : IAuthService
    {
        private readonly ApplicationDbContext _db = db;
        private readonly IConfiguration _config = config;

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(string username, CancellationToken ct = default)
        {
            var accessToken = GenerateAccessToken(username);

            var refreshToken = GenerateRefreshToken();
            var refreshEntity = new RefreshToken
            {
                Token = refreshToken,
                Username = username,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            _db.RefreshTokens.Add(refreshEntity);
            await _db.SaveChangesAsync(ct);

            return (accessToken, refreshToken);
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

            if (token == null || token.IsRevoked || token.IsUsed || token.Expires < DateTime.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            token.IsUsed = true; // rotate
            await _db.SaveChangesAsync(ct);

            return await GenerateTokensAsync(token.Username, ct);
        }

        private string GenerateAccessToken(string username)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
