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

public class CreateChallanDTO
{
    public int ApplicantId { get; set; }
    public int FeeTitleId { get; set; }
    public string? Details { get; set; }
}
