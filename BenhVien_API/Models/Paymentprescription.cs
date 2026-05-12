using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Paymentprescription
{
    public int PaymentId { get; set; }

    public int PrescriptionId { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
