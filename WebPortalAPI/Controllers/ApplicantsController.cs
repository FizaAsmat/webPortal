using Microsoft.AspNetCore.Mvc;
using WebPortalAPI.Models;  // adjust based on your namespace
using Microsoft.EntityFrameworkCore;

namespace WebPortalAPI.Controllers
{
    [ApiController]
    [Route("WebPortalAPI/Controllers")]
    public class ApplicantsController : ControllerBase
    {
        private readonly PmfdatabaseContext _context;

        public ApplicantsController(PmfdatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplicants()
        {
            var applicants = await _context.Applicants.ToListAsync();
            return Ok(applicants);
        }
    }
}
