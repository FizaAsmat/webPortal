[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly FeeTitleService _feeTitleService;

    public AdminController(FeeTitleService feeTitleService)
    {
        _feeTitleService = feeTitleService;
    }

    [HttpGet("feetitles")]
    public IActionResult GetFeeTitles()
    {
        var titles = _feeTitleService.GetAllFeeTitles();
        return Ok(titles);
    }

    // Add Create, Update, Delete Fee Title endpoints
}
