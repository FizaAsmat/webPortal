// File: RegisterDTO.cs
namespace WebPortalAPI.DTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class ApplicantRegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Cnic { get; set; }
        public string MobileNo { get; set; }
    }

    public class BankApprovalDTO
    {
        public int UserId { get; set; }
    }
}
