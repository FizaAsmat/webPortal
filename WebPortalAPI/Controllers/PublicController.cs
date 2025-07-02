using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPortalAPI.DTOs;
using WebPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebPortalAPI.Helpers;
using System.Text.Json;
using WebPortalAPI.Services;

namespace WebPortalAPI.Controllers
{
    [Route("api/public")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly PmfdatabaseContext _context;
        private readonly IConfiguration _config;
        private readonly PdfService _pdfService;

        public PublicController(
            PmfdatabaseContext context, 
            IConfiguration config,
            PdfService pdfService)
        {
            _context = context;
            _config = config;
            _pdfService = pdfService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] ApplicantRegisterDTO dto)
        {
            // Validate CNIC format (e.g., 12345-1234567-1)
            if (!Regex.IsMatch(dto.Cnic, @"^\d{5}-\d{7}-\d{1}$"))
            {
                return BadRequest(new { message = "Invalid CNIC format. Use format: 12345-1234567-1" });
            }

            // Validate mobile number (e.g., 03XX-XXXXXXX)
            if (!Regex.IsMatch(dto.MobileNo, @"^03\d{2}-\d{7}$"))
            {
                return BadRequest(new { message = "Invalid mobile number format. Use format: 03XX-XXXXXXX" });
            }

            // Check if CNIC already exists
            if (_context.Applicants.Any(a => a.Cnic == dto.Cnic))
            {
                return Conflict(new { message = "An applicant with this CNIC already exists." });
            }

            // Check if username exists
            if (_context.Users.Any(u => u.Username == dto.Username))
            {
                return Conflict(new { message = "Username already exists." });
            }

            // Create user account
            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                Role = "Public"
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Create applicant profile
            var applicant = new Applicant
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                Cnic = dto.Cnic,
                MobileNo = dto.MobileNo
            };
            _context.Applicants.Add(applicant);
            _context.SaveChanges();

            // Generate token
            var token = JwtTokenGenerator.GenerateToken(user, _config["Jwt:Key"]);

            return Ok(new
            {
                message = "Registration successful.",
                data = new
                {
                    token,
                    username = user.Username,
                    fullName = applicant.FullName,
                    cnic = applicant.Cnic,
                    mobileNo = applicant.MobileNo
                }
            });
        }

        [HttpPost("generate-challan")]
        [Authorize(Roles = "Public")]
        public async Task<IActionResult> GenerateChallan([FromBody] GenerateChallanDTO dto)
        {
            // Validate CNIC format
            if (!Regex.IsMatch(dto.Cnic, @"^\d{5}-\d{7}-\d{1}$"))
            {
                return BadRequest(new { message = "Invalid CNIC format. Use format: 12345-1234567-1" });
            }

            // Validate mobile number
            if (!Regex.IsMatch(dto.MobileNo, @"^03\d{2}-\d{7}$"))
            {
                return BadRequest(new { message = "Invalid mobile number format. Use format: 03XX-XXXXXXX" });
            }

            // Get or create applicant
            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(a => a.Cnic == dto.Cnic);

            if (applicant == null)
            {
                applicant = new Applicant
                {
                    FullName = dto.ApplicantName,
                    Cnic = dto.Cnic,
                    MobileNo = dto.MobileNo
                };
                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();
            }

            // Get fee title
            var feeTitle = await _context.FeeTitles
                .FirstOrDefaultAsync(f => f.FeeTitleId == dto.FeeTitleId);

            if (feeTitle == null)
            {
                return BadRequest(new { message = "Invalid fee title selected." });
            }

            // Calculate total amount and prepare details
            decimal totalAmount = feeTitle.Amount;
            string? details = null;

            // Handle Re-Checking case
            if (feeTitle.Title == "Re-Checking of Answer Sheet")
            {
                if (!dto.NumberOfSubjects.HasValue || dto.NumberOfSubjects <= 0)
                {
                    return BadRequest(new { message = "Number of subjects is required for re-checking." });
                }

                if (dto.SubjectNames == null || !dto.SubjectNames.Any() || 
                    dto.SubjectNames.Count != dto.NumberOfSubjects)
                {
                    return BadRequest(new { message = "Subject names are required and must match number of subjects." });
                }

                if (string.IsNullOrWhiteSpace(dto.Category))
                {
                    return BadRequest(new { message = "Category is required for re-checking." });
                }

                if (string.IsNullOrWhiteSpace(dto.RollNo))
                {
                    return BadRequest(new { message = "Roll number is required for re-checking." });
                }

                // Calculate total amount
                totalAmount = feeTitle.Amount * dto.NumberOfSubjects.Value;

                // Store re-checking details as JSON in Details column
                var recheckingDetails = new
                {
                    numberOfSubjects = dto.NumberOfSubjects.Value,
                    subjects = dto.SubjectNames,
                    category = dto.Category,
                    rollNo = dto.RollNo
                };
                details = JsonSerializer.Serialize(recheckingDetails);
            }

            // Create challan
            var challan = new Challan
            {
                ApplicantId = applicant.ApplicantId,
                FeeTitleId = feeTitle.FeeTitleId,
                FeeAmount = totalAmount,
                GeneratedDate = DateOnly.FromDateTime(DateTime.Now),
                IsPaid = false,
                IsExpired = false,
                Details = details
            };

            _context.Challans.Add(challan);
            await _context.SaveChangesAsync();

            // Prepare response
            var response = new
            {
                message = "Challan generated successfully.",
                data = new
                {
                    challanNo = challan.ChallanNo,
                    applicant = new
                    {
                        name = applicant.FullName,
                        cnic = applicant.Cnic,
                        mobileNo = applicant.MobileNo
                    },
                    fee = new
                    {
                        title = feeTitle.Title,
                        baseAmount = feeTitle.Amount,
                        totalAmount = challan.FeeAmount,
                        generatedDate = challan.GeneratedDate,
                        expiryDate = feeTitle.HasExpiry ? feeTitle.ExpiryDate : null
                    },
                    recheckingDetails = details != null ? JsonSerializer.Deserialize<object>(details) : null
                }
            };

            return Ok(response);
        }

        [HttpGet("fee-titles")]
        public async Task<IActionResult> GetFeeTitles()
        {
            var feeTitles = await _context.FeeTitles
                .Where(ft => !ft.HasExpiry || (ft.HasExpiry && ft.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now)))
                .Select(ft => new
                {
                    id = ft.FeeTitleId,
                    title = ft.Title,
                    amount = ft.Amount,
                    hasExpiry = ft.HasExpiry,
                    expiryDate = ft.ExpiryDate
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Fee titles retrieved successfully.",
                data = feeTitles
            });
        }

        [HttpGet("challan/download/{challanNo}")]
        [Authorize(Roles = "Public")]
        public async Task<IActionResult> DownloadChallan(string challanNo)
        {
            var challan = await _context.Challans
                .Include(c => c.Applicant)
                .Include(c => c.FeeTitle)
                .FirstOrDefaultAsync(c => c.ChallanNo.ToString() == challanNo);

            if (challan == null)
            {
                return NotFound(new { message = "Challan not found." });
            }

            // Verify that the challan belongs to the logged-in user
            var userId = User.FindFirst("UserId")?.Value;
            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(a => a.UserId.ToString() == userId);

            if (applicant == null || challan.ApplicantId != applicant.ApplicantId)
            {
                return Unauthorized(new { message = "You are not authorized to download this challan." });
            }

            try
            {
                var pdfBytes = _pdfService.GenerateChallanPdf(challan);
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"Challan-{challan.ChallanNo}.pdf"
                );
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { 
                    message = "An error occurred while generating the PDF.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("challan/status/{challanNo}")]
        [Authorize(Roles = "Public")]
        public async Task<IActionResult> GetChallanStatus(string challanNo)
        {
            var challan = await _context.Challans
                .Include(c => c.Applicant)
                .Include(c => c.FeeTitle)
                .Include(c => c.BankTransactions)
                .FirstOrDefaultAsync(c => c.ChallanNo.ToString() == challanNo);

            if (challan == null)
            {
                return NotFound(new { message = "Challan not found." });
            }

            // Verify that the challan belongs to the logged-in user
            var userId = User.FindFirst("UserId")?.Value;
            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(a => a.UserId.ToString() == userId);

            if (applicant == null || challan.ApplicantId != applicant.ApplicantId)
            {
                return Unauthorized(new { message = "You are not authorized to view this challan's status." });
            }

            // Get the latest bank transaction if paid
            var latestTransaction = challan.BankTransactions
                .OrderByDescending(bt => bt.TransactionDate)
                .FirstOrDefault();

            return Ok(new
            {
                message = "Challan status retrieved successfully.",
                data = new
                {
                    challanNo = challan.ChallanNo,
                    isPaid = challan.IsPaid ?? false,
                    isExpired = challan.IsExpired ?? false,
                    paidDate = latestTransaction?.TransactionDate,
                    fee = new
                    {
                        title = challan.FeeTitle.Title,
                        amount = challan.FeeAmount,
                        generatedDate = challan.GeneratedDate,
                        expiryDate = challan.FeeTitle.HasExpiry ? challan.FeeTitle.ExpiryDate : null
                    },
                    // Include re-checking details if present
                    recheckingDetails = !string.IsNullOrEmpty(challan.Details) && 
                                      challan.FeeTitle.Title == "Re-Checking of Answer Sheet" 
                        ? JsonSerializer.Deserialize<object>(challan.Details)
                        : null
                }
            });
        }
    }
}
