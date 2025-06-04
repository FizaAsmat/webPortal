using System;
using System.Collections.Generic;

namespace WebPortalAPI.Models;

public partial class Applicant
{
    public int ApplicantId { get; set; }

    public string FullName { get; set; } = null!;

    public string Cnic { get; set; } = null!;

    public string MobileNo { get; set; } = null!;

    public virtual ICollection<Challan> Challans { get; set; } = new List<Challan>();
}
