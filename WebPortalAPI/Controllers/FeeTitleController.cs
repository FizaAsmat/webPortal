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
        [HttpPost("create")]
        public IActionResult CreateFeeTitle([FromBody] FeeTitleDTO dto)
        {
            var feeTitle = new FeeTitle
            {
                Title = dto.Title,
                Amount = dto.Amount
            };

            _context.FeeTitles.Add(feeTitle);
            _context.SaveChanges();

            return Ok("Fee Title created successfully.");
        }

        // Get All
        [HttpGet("all")]
        public IActionResult GetAllFeeTitles()
        {
            var feeTitles = _context.FeeTitles
                .Select(ft => new FeeTitleDTO
                {
                    FeeTitleId = ft.FeeTitleId,
                    Title = ft.Title,
                    Amount = ft.Amount
                })
                .ToList();

            return Ok(feeTitles);
        }

        // Update
        [HttpPut("update/{id}")]
        public IActionResult UpdateFeeTitle(int id, [FromBody] FeeTitleDTO dto)
        {
            var feeTitle = _context.FeeTitles.FirstOrDefault(f => f.FeeTitleId == id);
            if (feeTitle == null)
                return NotFound("Fee Title not found.");

            feeTitle.Title = dto.Title;
            feeTitle.Amount = dto.Amount;
            _context.SaveChanges();

            return Ok("Fee Title updated successfully.");
        }

        // Delete
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteFeeTitle(int id)
        {
            var feeTitle = _context.FeeTitles.FirstOrDefault(f => f.FeeTitleId == id);
            if (feeTitle == null)
                return NotFound("Fee Title not found.");

            _context.FeeTitles.Remove(feeTitle);
            _context.SaveChanges();

            return Ok("Fee Title deleted successfully.");
        }
    }
}
