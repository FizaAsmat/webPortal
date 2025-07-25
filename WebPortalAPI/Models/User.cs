using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public User()
        {
            RefreshTokens = new List<RefreshToken>();
        }
    }
}
