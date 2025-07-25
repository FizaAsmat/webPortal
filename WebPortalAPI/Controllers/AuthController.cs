using Microsoft.AspNetCore.Mvc;
using WebPortalAPI.DTOs;
using WebPortalAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using WebPortalAPI.Services;

namespace WebPortalAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PmfdatabaseContext _context;
        private readonly IConfiguration _config;
        private readonly AuthService _authService;

        public AuthController(PmfdatabaseContext context, IConfiguration config, AuthService authService)
        {
            _context = context;
            _config = config;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenDTO>> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenDTO>> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDTO refreshTokenDto)
        {
            try
            {
                await _authService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public IActionResult Register([FromBody] RegisterDTO registerDto)
        {
            if (string.IsNullOrWhiteSpace(registerDto.Username) || 
                string.IsNullOrWhiteSpace(registerDto.Password) ||
                string.IsNullOrWhiteSpace(registerDto.Role))
            {
                return BadRequest("Username, password, and role are required.");
            }

            // Validate role
            if (!new[] { "Admin", "Bank", "Public" }.Contains(registerDto.Role))
            {
                return BadRequest("Invalid role. Role must be 'Admin', 'Bank', or 'Public'.");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == registerDto.Username);
            if (existingUser != null)
                return Conflict("Username already exists.");

            var newUser = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password,
                Role = registerDto.Role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User registered successfully.",
                data = new
                {
                    username = newUser.Username,
                    role = newUser.Role
                }
            });
        }
    }
}
