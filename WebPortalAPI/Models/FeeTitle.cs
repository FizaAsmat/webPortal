using System;
using System.Collections.Generic;

namespace WebPortalAPI.Models;

public partial class FeeTitle
{
    public int FeeTitleId { get; set; }

    public string Title { get; set; } = null!;

    public decimal Amount { get; set; }

    public bool HasExpiry { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public virtual ICollection<Challan> Challans { get; set; } = new List<Challan>();
}
