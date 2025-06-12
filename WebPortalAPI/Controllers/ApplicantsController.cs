[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly PmfdatabaseContext _context;

    public PublicController(PmfdatabaseContext context)
    {
        _context = context;
    }

    [HttpPost("generate-challan")]
    public IActionResult GenerateChallan([FromBody] ChallanDTO challanDto)
    {
        // validate, create new challan
        // generate PDF
        return Ok("Challan generated successfully");
    }

    [HttpGet("feetitles")]
    public IActionResult GetFeeTitles()
    {
        var titles = _context.FeeTitles
            .Select(f => new FeeTitleDTO
            {
                Id = f.Id,
                Title = f.Title,
                Amount = f.Amount,
                HasExpiry = f.HasExpiry,
                ExpiryDate = f.ExpiryDate
            }).ToList();

        return Ok(titles);
    }
}
