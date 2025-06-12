public class FeeTitleDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Amount { get; set; }
    public bool HasExpiry { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
