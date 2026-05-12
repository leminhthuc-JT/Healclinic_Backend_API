using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Email { get; set; }

    public string PassWord { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
