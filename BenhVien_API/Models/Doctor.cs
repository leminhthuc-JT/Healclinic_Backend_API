using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = null!;

    public string? SpecialtyId { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public TimeOnly? WorkStartTime { get; set; }

    public TimeOnly? WorkEndTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Chatwithdoctor> Chatwithdoctors { get; set; } = new List<Chatwithdoctor>();

    public virtual Specialty? Specialty { get; set; }

    public virtual User? User { get; set; }
}
