public class FeeTitleService
{
    private readonly PmfdatabaseContext _context;

    public FeeTitleService(PmfdatabaseContext context)
    {
        _context = context;
    }

    public List<FeeTitleDTO> GetAllFeeTitles()
    {
        return _context.FeeTitles
            .Select(f => new FeeTitleDTO
            {
                Id = f.Id,
                Title = f.Title,
                Amount = f.Amount,
                HasExpiry = f.HasExpiry,
                ExpiryDate = f.ExpiryDate
            })
            .ToList();
    }

    // Add CreateFeeTitle, UpdateFeeTitle methods
}
