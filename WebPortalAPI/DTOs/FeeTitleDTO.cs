namespace WebPortalAPI.DTOs
{
    public class FeeTitleDTO
    {
        public int? FeeTitleId { get; set; } // Nullable for create
        public string Title { get; set; }
        public decimal Amount { get; set; }
    }
}
