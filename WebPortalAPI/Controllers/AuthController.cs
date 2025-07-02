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

            // Validate bank-specific fields if registering a bank user
            if (registerDto.Role == "Bank")
            {
                if (string.IsNullOrWhiteSpace(registerDto.BankName) ||
                    string.IsNullOrWhiteSpace(registerDto.BranchCode) ||
                    string.IsNullOrWhiteSpace(registerDto.ContactPerson) ||
                    string.IsNullOrWhiteSpace(registerDto.ContactNumber) ||
                    string.IsNullOrWhiteSpace(registerDto.Email))
                {
                    return BadRequest("All bank details (name, branch code, contact person, contact number, and email) are required for bank users.");
                }
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == registerDto.Username);
            if (existingUser != null)
                return Conflict("Username already exists.");

            var newUser = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password,
                Role = registerDto.Role,
                // Set bank-specific fields only for bank users
                BankName = registerDto.Role == "Bank" ? registerDto.BankName : null,
                BranchCode = registerDto.Role == "Bank" ? registerDto.BranchCode : null,
                ContactPerson = registerDto.Role == "Bank" ? registerDto.ContactPerson : null,
                ContactNumber = registerDto.Role == "Bank" ? registerDto.ContactNumber : null,
                Email = registerDto.Role == "Bank" ? registerDto.Email : null
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User registered successfully.",
                data = new
                {
                    username = newUser.Username,
                    role = newUser.Role,
                    bankName = newUser.BankName
                }
            });
        }

        [HttpGet("pending-banks")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetPendingBanks()
        {
            var pendingBanks = _context.Users
                .Where(u => u.Role == "Bank" && !u.IsApproved)
                .Select(u => new
                {
                    userId = u.UserId,
                    username = u.Username,
                    bankName = u.BankName,
                    branchCode = u.BranchCode,
                    contactPerson = u.ContactPerson,
                    contactNumber = u.ContactNumber,
                    email = u.Email,
                    createdAt = u.CreatedAt
                })
                .OrderByDescending(u => u.createdAt)
                .ToList();

            return Ok(new
            {
                message = "Pending bank approvals retrieved successfully.",
                data = pendingBanks
            });
        }

        [HttpPost("approve-bank")]
        [Authorize(Roles = "Admin")]
        public IActionResult ApproveBankRegistration([FromBody] BankApprovalDTO approvalDto)
        {
            var user = _context.Users.FirstOrDefault(u => 
                u.UserId == approvalDto.UserId && 
                u.Role == "Bank" && 
                !u.IsApproved);

            if (user == null)
                return NotFound("Pending bank registration not found.");

            user.IsApproved = approvalDto.IsApproved;
            if (approvalDto.IsApproved)
            {
                user.ApprovedAt = DateTime.UtcNow;
                user.RejectionReason = null;
            }
            else
            {
                user.RejectionReason = approvalDto.RejectionReason;
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = approvalDto.IsApproved 
                    ? "Bank registration approved successfully." 
                    : "Bank registration rejected.",
                data = new
                {
                    username = user.Username,
                    bankName = user.BankName,
                    status = approvalDto.IsApproved ? "Approved" : "Rejected",
                    rejectionReason = user.RejectionReason
                }
            });
        }
    }
}
