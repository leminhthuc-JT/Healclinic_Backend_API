using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? PatientId { get; set; }

    public string? PaymentType { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Paymentappointment? Paymentappointment { get; set; }

    public virtual Paymentprescription? Paymentprescription { get; set; }

    public virtual ICollection<Paymentservice> Paymentservices { get; set; } = new List<Paymentservice>();
}
