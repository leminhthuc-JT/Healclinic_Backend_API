using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Paymentappointment
{
    public int PaymentId { get; set; }

    public int AppointmentId { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;
}
