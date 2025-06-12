using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebPortalAPI.Models;

public partial class User
{
[Key]
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Role { get; set; } // "Admin", "Bank", or "Public"
}
