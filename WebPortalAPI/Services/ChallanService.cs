using WebPortalAPI.Models;
using WebPortalAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WebPortalAPI.Services;
public class ChallanService
{
    private readonly PmfdatabaseContext _context;

    public ChallanService(PmfdatabaseContext context)
    {
        _context = context;
    }

    // Get a specific challan with details
    public ChallanDTO? GetChallanDetails(int challanNo)
    {
        var challan = _context.Challans
            .Include(c => c.Applicant)
            .Include(c => c.FeeTitle)
            .FirstOrDefault(c => c.ChallanNo == challanNo);

        if (challan == null) return null;

        return new ChallanDTO
        {
            ChallanNo = challan.ChallanNo,
            ApplicantId = challan.ApplicantId,
            ApplicantName = challan.Applicant.FullName,
            FeeTitle = challan.FeeTitle.Title,
            FeeAmount = challan.FeeAmount,
            ChallanDate = challan.GeneratedDate,
            IsPaid = challan.IsPaid ?? false,
            IsExpired = challan.IsExpired ?? false,
            Details = challan.Details
        };
    }

    // Get all challans
    public List<ChallanDTO> GetAllChallans()
    {
        return _context.Challans
            .Include(c => c.Applicant)
            .Include(c => c.FeeTitle)
            .Select(c => new ChallanDTO
            {
                ChallanNo = c.ChallanNo,
                ApplicantId = c.ApplicantId,
                ApplicantName = c.Applicant.FullName,
                FeeTitle = c.FeeTitle.Title,
                FeeAmount = c.FeeAmount,
                ChallanDate = c.GeneratedDate,
                IsPaid = c.IsPaid ?? false,
                IsExpired = c.IsExpired ?? false,
                Details = c.Details
            })
            .ToList();
    }

    // Create a new challan
    public async Task<ChallanDTO> CreateChallanAsync(CreateChallanDTO dto)
    {
        var feeTitle = await _context.FeeTitles.FindAsync(dto.FeeTitleId);
        if (feeTitle == null) throw new Exception("Invalid FeeTitleId.");

        var challan = new Challan
        {
            ApplicantId = dto.ApplicantId,
            FeeTitleId = dto.FeeTitleId,
            FeeAmount = feeTitle.Amount,
            GeneratedDate = DateOnly.FromDateTime(DateTime.Now),
            IsPaid = false,
            IsExpired = false,
            Details = dto.Details
        };

        _context.Challans.Add(challan);
        await _context.SaveChangesAsync();

        return new ChallanDTO
        {
            ChallanNo = challan.ChallanNo,
            ApplicantId = challan.ApplicantId,
            ApplicantName = _context.Applicants.Find(dto.ApplicantId)?.FullName ?? "Unknown",
            FeeTitle = feeTitle.Title,
            FeeAmount = challan.FeeAmount,
            ChallanDate = challan.GeneratedDate,
            IsPaid = false,
            IsExpired = false,
            Details = dto.Details
        };
    }

    // Mark challan as paid
    public async Task<bool> MarkAsPaidAsync(int challanNo)
    {
        var challan = await _context.Challans.FindAsync(challanNo);
        if (challan == null) return false;

        challan.IsPaid = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
