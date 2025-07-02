namespace WebPortalAPI.DTOs;

public class ChallanDTO
{
    public int ChallanNo { get; set; }
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = "";
    public string FeeTitle { get; set; } = "";
    public decimal FeeAmount { get; set; }
    public DateOnly ChallanDate { get; set; }
    public bool IsPaid { get; set; }
    public bool IsExpired { get; set; }
    public string? Details { get; set; }
}

public class GenerateChallanDTO
{
    public string ApplicantName { get; set; }
    public string Cnic { get; set; }
    public string MobileNo { get; set; }
    public int FeeTitleId { get; set; }
    // Re-Checking specific fields (optional)
    public int? NumberOfSubjects { get; set; }
    public List<string>? SubjectNames { get; set; }
    public string? Category { get; set; }
    public string? RollNo { get; set; }
}

public class CreateChallanDTO
{
    public int ApplicantId { get; set; }
    public int FeeTitleId { get; set; }
    public string? Details { get; set; }
}
