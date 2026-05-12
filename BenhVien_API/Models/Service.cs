using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Service
{
    public string ServiceId { get; set; } = null!;

    public string ServiceName { get; set; } = null!;

    public decimal? Price { get; set; }

    public string? Department { get; set; }

    public string? Description { get; set; }

    public bool? IsCover { get; set; }

    public string SpecialtyId { get; set; } = null!;

    public virtual ICollection<Paymentservice> Paymentservices { get; set; } = new List<Paymentservice>();

    public virtual Specialty Specialty { get; set; } = null!;
}
