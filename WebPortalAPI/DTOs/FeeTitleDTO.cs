namespace WebPortalAPI.DTOs
{
    public class FeeTitleDTO
    {
        public int? FeeTitleId { get; set; } // Nullable for create
        public string Title { get; set; }
        public decimal Amount { get; set; }

        // ✅ Add these two properties:
        public bool HasExpiry { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
