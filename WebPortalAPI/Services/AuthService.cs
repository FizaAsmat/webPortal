using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebPortalAPI.DTOs;
using WebPortalAPI.Helpers;
using WebPortalAPI.Models;

namespace WebPortalAPI.Services
{
    public class AuthService
    {
        private readonly PmfdatabaseContext _context;
        private readonly string _jwtKey;

        public AuthService(PmfdatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"];
        }

        public async Task<TokenDTO> LoginAsync(LoginDTO loginDto)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || user.Password != loginDto.Password)
            {
                throw new Exception("Invalid username or password");
            }

            var tokens = JwtTokenGenerator.GenerateTokens(user, _jwtKey);

            // Save refresh token
            var refreshToken = new RefreshToken
            {
                Token = tokens.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedDate = DateTime.UtcNow,
                IsRevoked = false,
                UserId = user.Id
            };

            user.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return tokens;
        }

        public async Task<TokenDTO> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                throw new Exception("Invalid refresh token");
            }

            if (storedToken.ExpiryDate < DateTime.UtcNow || storedToken.IsRevoked)
            {
                throw new Exception("Refresh token has expired or been revoked");
            }

            var user = storedToken.User;
            var newTokens = JwtTokenGenerator.GenerateTokens(user, _jwtKey);

            // Revoke old refresh token
            storedToken.IsRevoked = true;

            // Save new refresh token
            var newRefreshToken = new RefreshToken
            {
                Token = newTokens.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedDate = DateTime.UtcNow,
                IsRevoked = false,
                UserId = user.Id
            };

            user.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return newTokens;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
