using System;
using System.Collections.Generic;

namespace WebPortalAPI.Models;

public partial class BankTransaction
{
    public int TransactionId { get; set; }

    public int ChallanNo { get; set; }

    public DateOnly ChallanDate { get; set; }

    public decimal ChallanAmount { get; set; }

    public string? FeeTitle { get; set; }

    public DateOnly PaidDate { get; set; }

    public string? BranchName { get; set; }

    public string? BranchCode { get; set; }

    public virtual Challan ChallanNoNavigation { get; set; } = null!;
}
