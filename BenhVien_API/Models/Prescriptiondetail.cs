using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Prescriptiondetail
{
    public int PrescriptionId { get; set; }

    public string MedicationId { get; set; } = null!;

    public int? Duration { get; set; }

    public int? Quantity { get; set; }

    public string? Note { get; set; }

    public virtual Medication Medication { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
