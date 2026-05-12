using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Medication
{
    public string MedicationId { get; set; } = null!;

    public string MedicineName { get; set; } = null!;

    public decimal? Dosage { get; set; }

    public int? Frequency { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Quantity { get; set; }

    public string? Unit { get; set; }

    public string? Country { get; set; }

    public string? Manufacturer { get; set; }

    public decimal? Price { get; set; }

    public bool? IsCover { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<Prescriptiondetail> Prescriptiondetails { get; set; } = new List<Prescriptiondetail>();
}
