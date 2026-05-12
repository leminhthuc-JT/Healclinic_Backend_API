using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? BloodType { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? Allergies { get; set; }

    public string? ChronicDiseases { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Chatwithdoctor> Chatwithdoctors { get; set; } = new List<Chatwithdoctor>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
