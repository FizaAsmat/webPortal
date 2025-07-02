using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebPortalAPI.DTOs;
using WebPortalAPI.Models;

namespace WebPortalAPI.Controllers
{
    [Route("api/feetitle")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FeeTitleController : ControllerBase
    {
        private readonly PmfdatabaseContext _context;

        public FeeTitleController(PmfdatabaseContext context)
        {
            _context = context;
        }

        // Create
        [HttpPost]
        public IActionResult CreateFeeTitle([FromBody] FeeTitleDTO dto)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("Title is required.");
            }

            if (dto.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            // Validate expiry date if HasExpiry is true
            if (dto.HasExpiry)
            {
                if (!dto.ExpiryDate.HasValue)
                {
                    return BadRequest("Expiry date is required when HasExpiry is true.");
                }

                if (dto.ExpiryDate.Value.Date <= DateTime.Now.Date)
                {
                    return BadRequest("Expiry date must be a future date.");
                }
            }

            var feeTitle = new FeeTitle
            {
                Title = dto.Title,
                Amount = dto.Amount,
                HasExpiry = dto.HasExpiry,
                ExpiryDate = dto.HasExpiry && dto.ExpiryDate.HasValue 
                    ? DateOnly.FromDateTime(dto.ExpiryDate.Value) 
                    : null
            };

            _context.FeeTitles.Add(feeTitle);
            _context.SaveChanges();

            return Ok(new { 
                message = "Fee Title created successfully.",
                data = new FeeTitleDTO
                {
                    FeeTitleId = feeTitle.FeeTitleId,
                    Title = feeTitle.Title,
                    Amount = feeTitle.Amount,
                    HasExpiry = feeTitle.HasExpiry,
                    ExpiryDate = feeTitle.ExpiryDate?.ToDateTime(TimeOnly.MinValue)
                }
            });
        }

        // Get All
        [HttpGet]
        public IActionResult GetAllFeeTitles()
        {
            var feeTitles = _context.FeeTitles
                .Select(ft => new FeeTitleDTO
                {
                    FeeTitleId = ft.FeeTitleId,
                    Title = ft.Title,
                    Amount = ft.Amount,
                    HasExpiry = ft.HasExpiry,
                    ExpiryDate = ft.ExpiryDate?.ToDateTime(TimeOnly.MinValue)
                })
                .ToList();

            return Ok(new { 
                message = "Fee Titles retrieved successfully.",
                data = feeTitles 
            });
        }

        // Update
        [HttpPut("{id}")]
        public IActionResult UpdateFeeTitle(int id, [FromBody] FeeTitleDTO dto)
        {
            var feeTitle = _context.FeeTitles.FirstOrDefault(f => f.FeeTitleId == id);
            if (feeTitle == null)
                return NotFound("Fee Title not found.");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("Title is required.");
            }

            if (dto.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            // Validate expiry date if HasExpiry is true
            if (dto.HasExpiry)
            {
                if (!dto.ExpiryDate.HasValue)
                {
                    return BadRequest("Expiry date is required when HasExpiry is true.");
                }

                if (dto.ExpiryDate.Value.Date <= DateTime.Now.Date)
                {
                    return BadRequest("Expiry date must be a future date.");
                }
            }

            feeTitle.Title = dto.Title;
            feeTitle.Amount = dto.Amount;
            feeTitle.HasExpiry = dto.HasExpiry;
            feeTitle.ExpiryDate = dto.HasExpiry && dto.ExpiryDate.HasValue 
                ? DateOnly.FromDateTime(dto.ExpiryDate.Value) 
                : null;

            _context.SaveChanges();

            return Ok(new { 
                message = "Fee Title updated successfully.",
                data = new FeeTitleDTO
                {
                    FeeTitleId = feeTitle.FeeTitleId,
                    Title = feeTitle.Title,
                    Amount = feeTitle.Amount,
                    HasExpiry = feeTitle.HasExpiry,
                    ExpiryDate = feeTitle.ExpiryDate?.ToDateTime(TimeOnly.MinValue)
                }
            });
        }

        // Delete
        [HttpDelete("{id}")]
        public IActionResult DeleteFeeTitle(int id)
        {
            var feeTitle = _context.FeeTitles.FirstOrDefault(f => f.FeeTitleId == id);
            if (feeTitle == null)
                return NotFound("Fee Title not found.");

            _context.FeeTitles.Remove(feeTitle);
            _context.SaveChanges();

            return Ok(new { 
                message = "Fee Title deleted successfully.",
                data = new FeeTitleDTO
                {
                    FeeTitleId = feeTitle.FeeTitleId,
                    Title = feeTitle.Title,
                    Amount = feeTitle.Amount,
                    HasExpiry = feeTitle.HasExpiry,
                    ExpiryDate = feeTitle.ExpiryDate?.ToDateTime(TimeOnly.MinValue)
                }
            });
        }
    }
}
