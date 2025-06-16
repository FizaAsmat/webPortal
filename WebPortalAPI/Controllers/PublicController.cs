using Microsoft.AspNetCore.Mvc;
using WebPortalAPI.Models;
using WebPortalAPI.Services;
using WebPortalAPI.DTOs;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly PmfdatabaseContext _context;
    private readonly ChallanService _challanService;

    public PublicController(PmfdatabaseContext context, ChallanService challanService)
    {
        _context = context;
        _challanService = challanService;
    }

    // POST: api/public/generate-challan
    [HttpPost("generate-challan")]
    public async Task<IActionResult> GenerateChallan([FromBody] CreateChallanDTO challanDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var challan = await _challanService.CreateChallanAsync(challanDto);

            // PDF generation can go here in future
            return Ok(new
            {
                message = "Challan generated successfully.",
                challan
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/public/feetitles
    [HttpGet("feetitles")]
    public IActionResult GetFeeTitles()
    {
        var titles = _context.FeeTitles
            .Select(f => new FeeTitleDTO
            {
                FeeTitleId = f.FeeTitleId,
                Title = f.Title,
                Amount = f.Amount,
                HasExpiry = f.HasExpiry,
                ExpiryDate = f.ExpiryDate
            }).ToList();

        return Ok(titles);
    }
}
