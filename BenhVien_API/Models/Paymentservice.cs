using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Paymentservice
{
    public int PaymentId { get; set; }

    public string ServiceId { get; set; } = null!;

    public int? Quantity { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
