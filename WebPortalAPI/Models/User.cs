using System.ComponentModel.DataAnnotations;

namespace WebPortalAPI.Models
{
    public partial class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!; // "Admin", "Bank", or "Public"
    }
}
