public class ChallanService
{
    private readonly PmfdatabaseContext _context;

    public ChallanService(PmfdatabaseContext context)
    {
        _context = context;
    }

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
            ApplicantName = challan.Applicant.ApplicantName,
            FeeTitle = challan.FeeTitle.Title,
            FeeAmount = challan.FeeTitle.Amount,
            ChallanDate = challan.ChallanDate,
            PaidDate = challan.PaidDate,
            IsPaid = challan.IsPaid
        };
    }

    // Add methods like MarkAsPaid(), CreateChallan(), etc.
}
