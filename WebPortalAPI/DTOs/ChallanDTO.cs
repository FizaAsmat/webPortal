public class ChallanDTO
{
    public int ChallanNo { get; set; }
    public string ApplicantName { get; set; }
    public string FeeTitle { get; set; }
    public decimal FeeAmount { get; set; }
    public DateTime ChallanDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsPaid { get; set; }
}
