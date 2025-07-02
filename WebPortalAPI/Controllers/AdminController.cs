using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebPortalAPI.Services;
using WebPortalAPI.DTOs;
using WebPortalAPI.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly FeeTitleService _feeTitleService;
    private readonly PmfdatabaseContext _context;
    private readonly ChallanService _challanService;

    public AdminController(
        FeeTitleService feeTitleService,
        PmfdatabaseContext context,
        ChallanService challanService)
    {
        _feeTitleService = feeTitleService;
        _context = context;
        _challanService = challanService;
    }

    #region Fee Titles Management
    [HttpGet("feetitles")]
    public IActionResult GetFeeTitles()
    {
        var titles = _feeTitleService.GetAllFeeTitles();
        return Ok(titles);
    }

    [HttpGet("feetitles/{id}")]
    public async Task<IActionResult> GetFeeTitle(int id)
    {
        var title = await _feeTitleService.GetFeeTitleByIdAsync(id);
        if (title == null)
            return NotFound();
        return Ok(title);
    }

    [HttpPost("feetitles")]
    public async Task<IActionResult> CreateFeeTitle([FromBody] FeeTitleDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _feeTitleService.CreateFeeTitleAsync(dto);
        return CreatedAtAction(nameof(GetFeeTitle), new { id = result.FeeTitleId }, result);
    }

    [HttpPut("feetitles/{id}")]
    public async Task<IActionResult> UpdateFeeTitle(int id, [FromBody] FeeTitleDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _feeTitleService.UpdateFeeTitleAsync(id, dto);
        if (!success)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("feetitles/{id}")]
    public async Task<IActionResult> DeleteFeeTitle(int id)
    {
        var success = await _feeTitleService.DeleteFeeTitleAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
    #endregion

    #region User Management
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.UserId, u.Username, u.Role })
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.UserId == id)
            .Select(u => new { u.UserId, u.Username, u.Role })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string newRole)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Validate role
        if (!new[] { "Admin", "User" }.Contains(newRole))
            return BadRequest("Invalid role specified");

        user.Role = newRole;
        await _context.SaveChangesAsync();
        return NoContent();
    }
    #endregion

    #region Dashboard Statistics
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = new
        {
            TotalApplicants = await _context.Applicants.CountAsync(),
            TotalFeeTitles = await _context.FeeTitles.CountAsync(),
            TotalChallans = await _context.Challans.CountAsync(),
            PaidChallans = await _context.Challans.CountAsync(c => c.IsPaid),
            TotalRevenue = await _context.BankTransactions.SumAsync(bt => bt.ChallanAmount),
            ActiveFeeTitles = await _context.FeeTitles.CountAsync(ft => 
                !ft.HasExpiry || (ft.HasExpiry && ft.ExpiryDate >= DateOnly.FromDateTime(DateTime.Today)))
        };
        return Ok(stats);
    }
    #endregion

    #region Challan Management
    [HttpGet("challans")]
    public async Task<IActionResult> GetAllChallans()
    {
        var challans = await _context.Challans
            .Include(c => c.Applicant)
            .Include(c => c.FeeTitle)
            .Include(c => c.BankTransactions)
            .Select(c => new
            {
                ChallanNo = c.ChallanNo,
                ApplicantName = c.Applicant.FullName,
                FeeTitle = c.FeeTitle.Title,
                Amount = c.FeeAmount,
                GeneratedDate = c.GeneratedDate,
                IsPaid = c.IsPaid ?? false,
                IsExpired = c.IsExpired ?? false,
                ExpiryDate = c.FeeTitle.HasExpiry ? c.FeeTitle.ExpiryDate : null,
                PaymentDate = c.BankTransactions
                    .OrderByDescending(bt => bt.TransactionDate)
                    .Select(bt => bt.TransactionDate)
                    .FirstOrDefault()
            })
            .OrderByDescending(c => c.GeneratedDate)
            .ToListAsync();

        return Ok(new { 
            message = "Challans retrieved successfully.",
            data = challans 
        });
    }

    [HttpGet("challans/search")]
    public async Task<IActionResult> SearchChallans(
        [FromQuery] string? challanNo,
        [FromQuery] string? applicantName,
        [FromQuery] string? feeTitle)
    {
        var query = _context.Challans
            .Include(c => c.Applicant)
            .Include(c => c.FeeTitle)
            .Include(c => c.BankTransactions)
            .AsQueryable();

        // Apply filters if provided
        if (!string.IsNullOrWhiteSpace(challanNo))
        {
            query = query.Where(c => c.ChallanNo.ToString().Contains(challanNo));
        }

        if (!string.IsNullOrWhiteSpace(applicantName))
        {
            query = query.Where(c => c.Applicant.FullName.Contains(applicantName));
        }

        if (!string.IsNullOrWhiteSpace(feeTitle))
        {
            query = query.Where(c => c.FeeTitle.Title.Contains(feeTitle));
        }

        var challans = await query
            .Select(c => new
            {
                ChallanNo = c.ChallanNo,
                ApplicantName = c.Applicant.FullName,
                FeeTitle = c.FeeTitle.Title,
                Amount = c.FeeAmount,
                GeneratedDate = c.GeneratedDate,
                IsPaid = c.IsPaid ?? false,
                IsExpired = c.IsExpired ?? false,
                ExpiryDate = c.FeeTitle.HasExpiry ? c.FeeTitle.ExpiryDate : null,
                PaymentDate = c.BankTransactions
                    .OrderByDescending(bt => bt.TransactionDate)
                    .Select(bt => bt.TransactionDate)
                    .FirstOrDefault()
            })
            .OrderByDescending(c => c.GeneratedDate)
            .ToListAsync();

        return Ok(new { 
            message = "Challans search completed successfully.",
            data = challans 
        });
    }

    [HttpGet("challans/{challanNo}")]
    public async Task<IActionResult> GetChallanDetails(string challanNo)
    {
        var challan = await _context.Challans
            .Include(c => c.Applicant)
            .Include(c => c.FeeTitle)
            .Include(c => c.BankTransactions)
            .FirstOrDefaultAsync(c => c.ChallanNo == challanNo);

        if (challan == null)
            return NotFound();

        return Ok(new
        {
            challan.ChallanNo,
            challan.GeneratedDate,
            challan.FeeAmount,
            challan.IsPaid,
            challan.IsExpired,
            Applicant = new
            {
                challan.Applicant.ApplicantId,
                challan.Applicant.FullName,
                challan.Applicant.Cnic,
                challan.Applicant.MobileNo
            },
            FeeTitle = new
            {
                challan.FeeTitle.FeeTitleId,
                challan.FeeTitle.Title
            },
            BankTransactions = challan.BankTransactions.Select(bt => new
            {
                bt.TransactionId,
                bt.BranchCode,
                bt.BranchName,
                bt.ChallanAmount,
                bt.TransactionDate
            })
        });
    }

    [HttpPost("challans/{challanNo}/mark-expired")]
    public async Task<IActionResult> MarkChallanAsExpired(string challanNo)
    {
        var challan = await _context.Challans.FindAsync(challanNo);
        if (challan == null)
            return NotFound();

        if (challan.IsPaid)
            return BadRequest("Cannot mark a paid challan as expired");

        challan.IsExpired = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }
    #endregion

    #region Bank Transactions
    [HttpGet("transactions")]
    public async Task<IActionResult> GetBankTransactions([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = _context.BankTransactions
            .Include(bt => bt.ChallanNoNavigation)
            .ThenInclude(c => c.Applicant)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(bt => bt.TransactionDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(bt => bt.TransactionDate <= toDate.Value);

        var transactions = await query
            .Select(bt => new
            {
                bt.TransactionId,
                bt.ChallanNo,
                bt.BranchCode,
                bt.BranchName,
                bt.ChallanAmount,
                bt.TransactionDate,
                Applicant = new
                {
                    bt.ChallanNoNavigation.Applicant.FullName,
                    bt.ChallanNoNavigation.Applicant.Cnic
                }
            })
            .OrderByDescending(bt => bt.TransactionDate)
            .ToListAsync();

        return Ok(transactions);
    }
    #endregion
}
