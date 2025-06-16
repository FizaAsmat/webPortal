namespace WebPortalAPI.DTOs;

public class FeeTitleDTO
{
    public int FeeTitleId { get; set; }
    public string Title { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool HasExpiry { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
