using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Specialty
{
    public string SpecialtyId { get; set; } = null!;

    public string SpecialtyName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
