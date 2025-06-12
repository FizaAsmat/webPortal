[ApiController]
[Route("api/bank")]
[Authorize(Roles = "Bank")]
public class BankController : ControllerBase
{
    private readonly ChallanService _challanService;

    public BankController(ChallanService challanService)
    {
        _challanService = challanService;
    }

    [HttpGet("challan/{challanNo}")]
    public IActionResult GetChallan(int challanNo)
    {
        var challan = _challanService.GetChallanDetails(challanNo);
        if (challan == null)
            return NotFound("No Challan found");

        if (challan.IsPaid)
            return BadRequest("Challan already paid");

        return Ok(challan);
    }

    // Add MarkChallanAsPaid()
}
