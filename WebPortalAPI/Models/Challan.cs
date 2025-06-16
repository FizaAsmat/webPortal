using System;
using System.Collections.Generic;

namespace WebPortalAPI.Models
{
    public partial class Challan
    {
        public int ChallanNo { get; set; }

        public int ApplicantId { get; set; }

        public int FeeTitleId { get; set; }

        public decimal FeeAmount { get; set; }

        public bool? IsPaid { get; set; }

        public bool? IsExpired { get; set; }

        public DateOnly GeneratedDate { get; set; }

        public string? Details { get; set; }

        public virtual Applicant Applicant { get; set; } = null!;

        public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();

        public virtual FeeTitle FeeTitle { get; set; } = null!;
    }
}
