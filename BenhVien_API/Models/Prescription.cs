using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Prescription
{
    public int PrescriptionId { get; set; }

    public int RecordId { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Paymentprescription> Paymentprescriptions { get; set; } = new List<Paymentprescription>();

    public virtual ICollection<Prescriptiondetail> Prescriptiondetails { get; set; } = new List<Prescriptiondetail>();

    public virtual MedicalRecord Record { get; set; } = null!;
}
