using Microsoft.AspNetCore.Mvc;
using WebPortalAPI.DTOs;
using WebPortalAPI.Helpers;
using WebPortalAPI.Models;

namespace WebPortalAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PmfdatabaseContext _context;
        private readonly IConfiguration _config;

        public AuthController(PmfdatabaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginDto)
        {
            var user = _context.Users.FirstOrDefault(u => 
                u.Username == loginDto.Username && 
                u.Password == loginDto.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = JwtTokenGenerator.GenerateToken(user, _config["Jwt:Key"]);

            return Ok(new
            {
                token,
                role = user.Role
            });
        }
    }
}
