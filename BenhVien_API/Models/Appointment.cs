using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string SpecialtyId { get; set; } = null!;

    public string? TimeExpected { get; set; }

    public int? QueueNumber { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public bool? IsCover { get; set; }

    public decimal? Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Paymentappointment> Paymentappointments { get; set; } = new List<Paymentappointment>();

    public virtual Specialty Specialty { get; set; } = null!;
}
