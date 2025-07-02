using System.ComponentModel.DataAnnotations;

namespace WebPortalAPI.DTOs
{
    public class FeeTitleDTO
    {
        public int? FeeTitleId { get; set; } // Nullable for create

        [Required(ErrorMessage = "Fee Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Fee Title must be between 3 and 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Fee Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fee Amount must be greater than 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Fee Amount must be a valid number with up to 2 decimal places")]
        public decimal Amount { get; set; }

        // ✅ Add these two properties:
        public bool HasExpiry { get; set; }

        [CustomFutureDateValidation(ErrorMessage = "Expiry Date must be a future date")]
        public DateOnly? ExpiryDate { get; set; }
    }

    public class CustomFutureDateValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return true;

            if (value is DateOnly date)
            {
                return date > DateOnly.FromDateTime(DateTime.Today);
            }

            return false;
        }
    }
}
