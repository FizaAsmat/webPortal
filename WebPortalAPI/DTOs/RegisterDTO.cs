// File: RegisterDTO.cs
namespace WebPortalAPI.DTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        // Bank specific fields - only used when admin creates a bank user
        public string? BankName { get; set; }
        public string? BranchCode { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
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
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
