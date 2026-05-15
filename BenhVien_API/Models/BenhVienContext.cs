using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BenhVien_API.Models;

public partial class BenhVienContext : DbContext
{
    public BenhVienContext()
    {
    }

    public BenhVienContext(DbContextOptions<BenhVienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Chatwithdoctor> Chatwithdoctors { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<Messagedetail> Messagedetails { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Paymentappointment> Paymentappointments { get; set; }

    public virtual DbSet<Paymentprescription> Paymentprescriptions { get; set; }

    public virtual DbSet<Paymentservice> Paymentservices { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Prescriptiondetail> Prescriptiondetails { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=JOHNNY;Initial Catalog=QL_BENHVIEN;User ID=sa;Password=Minhthuc@1234;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("pk_app");

            entity.ToTable("APPOINTMENTS", tb => tb.HasTrigger("TG_UPDATE_APPOINTMENT"));

            entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");
            entity.Property(e => e.AppointmentDate)
                .HasColumnType("datetime")
                .HasColumnName("appointmentDate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DoctorId).HasColumnName("doctorId");
            entity.Property(e => e.IsCover)
                .HasDefaultValue(false)
                .HasColumnName("isCover");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("price");
            entity.Property(e => e.QueueNumber).HasColumnName("queueNumber");
            entity.Property(e => e.SpecialtyId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("specialtyId");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TimeExpected)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("timeExpected");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("updateAt");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_app_dt");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_app_pt");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_app_sp");
        });

        modelBuilder.Entity<Chatwithdoctor>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("pk_c");

            entity.ToTable("CHATWITHDOCTOR");

            entity.HasIndex(e => new { e.PatientId, e.DoctorId }, "unq_c").IsUnique();

            entity.Property(e => e.ChatId).HasColumnName("chatId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DoctorId).HasColumnName("doctorId");
            entity.Property(e => e.PatientId).HasColumnName("patientId");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Chatwithdoctors)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_c_dt");

            entity.HasOne(d => d.Patient).WithMany(p => p.Chatwithdoctors)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_c_pt");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("pk_dt");

            entity.ToTable("DOCTORS", tb => tb.HasTrigger("TG_UPDATE_DOCTOR"));

            entity.Property(e => e.DoctorId).HasColumnName("doctorId");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasDefaultValue("default_image.png")
                .IsFixedLength()
                .HasColumnName("avatarUrl");
            entity.Property(e => e.Bio)
                .HasMaxLength(500)
                .HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExperienceYears).HasColumnName("experienceYears");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("fullName");
            entity.Property(e => e.SpecialtyId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("specialtyId");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("updateAt");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.WorkEndTime).HasColumnName("workEndTime");
            entity.Property(e => e.WorkStartTime).HasColumnName("workStartTime");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .HasConstraintName("fk_dt_spc");

            entity.HasOne(d => d.User).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("pk_dt_us");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("pk_mr");

            entity.ToTable("MEDICAL_RECORDS");

            entity.Property(e => e.RecordId).HasColumnName("recordId");
            entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Diagnosis)
                .HasMaxLength(200)
                .HasColumnName("diagnosis");
            entity.Property(e => e.IsCover).HasColumnName("isCover");
            entity.Property(e => e.PercentCover).HasColumnName("percentCover");
            entity.Property(e => e.Symptoms)
                .HasMaxLength(200)
                .HasColumnName("symptoms");
            entity.Property(e => e.Treatment)
                .HasMaxLength(200)
                .HasColumnName("treatment");

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_mr_app");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.MedicationId).HasName("pk_mdc");

            entity.ToTable("MEDICATIONS");

            entity.HasIndex(e => e.MedicineName, "UQ__MEDICATI__722522D7B2A09A49").IsUnique();

            entity.Property(e => e.MedicationId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("medicationId");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.Dosage)
                .HasColumnType("decimal(2, 1)")
                .HasColumnName("dosage");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("endDate");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.IsCover).HasColumnName("isCover");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(200)
                .HasColumnName("manufacturer");
            entity.Property(e => e.MedicineName)
                .HasMaxLength(200)
                .HasColumnName("medicineName");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("startDate");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<Messagedetail>(entity =>
        {
            entity.HasKey(e => new { e.ChatId, e.MessageId }).HasName("pk_mess");

            entity.ToTable("MESSAGEDETAIL");

            entity.Property(e => e.ChatId).HasColumnName("chatId");
            entity.Property(e => e.MessageId)
                .ValueGeneratedOnAdd()
                .HasColumnName("messageId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasDefaultValue("default_image.png")
                .HasColumnName("imageUrl");
            entity.Property(e => e.MessageText)
                .HasMaxLength(2000)
                .HasColumnName("messageText");
            entity.Property(e => e.ReceiverId).HasColumnName("receiverId");
            entity.Property(e => e.SenderId).HasColumnName("senderId");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messagedetails)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_mess_c");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("pk_pt");

            entity.ToTable("PATIENTS", tb => tb.HasTrigger("TG_UPDATE_PATIENT"));

            entity.HasIndex(e => e.Phone, "UQ__PATIENTS__B43B145FB394B768").IsUnique();

            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.Allergies)
                .HasMaxLength(200)
                .HasColumnName("allergies");
            entity.Property(e => e.BloodType)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("bloodType");
            entity.Property(e => e.ChronicDiseases)
                .HasMaxLength(200)
                .HasColumnName("chronicDiseases");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .HasColumnName("fullName");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Height)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("height");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("updateAt");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Weight)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weight");

            entity.HasOne(d => d.User).WithMany(p => p.Patients)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("pk_pt_us");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("pk_pm");

            entity.ToTable("PAYMENT", tb => tb.HasTrigger("TG_UPDATE_PAYMENT"));

            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(100)
                .HasColumnName("paymentType");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("totalAmount");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("updateAt");

            entity.HasOne(d => d.Patient).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("fk_pm_pt");
        });

        modelBuilder.Entity<Paymentappointment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("pk_paa");

            entity.ToTable("PAYMENTAPPOINTMENT", tb => tb.HasTrigger("TG_TOTALAMOUNT_APPOINTMENT"));

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("paymentId");
            entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Paymentappointments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_paa_ps");

            entity.HasOne(d => d.Payment).WithOne(p => p.Paymentappointment)
                .HasForeignKey<Paymentappointment>(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_paa_pm");
        });

        modelBuilder.Entity<Paymentprescription>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("pk_pap");

            entity.ToTable("PAYMENTPRESCRIPTION", tb => tb.HasTrigger("TG_TOTALAMOUNT_PRESCRIPTION"));

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("paymentId");
            entity.Property(e => e.PrescriptionId).HasColumnName("prescriptionId");

            entity.HasOne(d => d.Payment).WithOne(p => p.Paymentprescription)
                .HasForeignKey<Paymentprescription>(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pap_pm");

            entity.HasOne(d => d.Prescription).WithMany(p => p.Paymentprescriptions)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pap_ps");
        });

        modelBuilder.Entity<Paymentservice>(entity =>
        {
            entity.HasKey(e => new { e.PaymentId, e.ServiceId }).HasName("pk_pas");

            entity.ToTable("PAYMENTSERVICE", tb =>
                {
                    tb.HasTrigger("TG_QUANTITY_SERVICE");
                    tb.HasTrigger("TG_TOTALAMOUNT_SERVICE");
                });

            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("serviceId");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.HasOne(d => d.Payment).WithMany(p => p.Paymentservices)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pas_pm");

            entity.HasOne(d => d.Service).WithMany(p => p.Paymentservices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pas_sv");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("pk_ps");

            entity.ToTable("PRESCRIPTIONS");

            entity.Property(e => e.PrescriptionId).HasColumnName("prescriptionId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.RecordId).HasColumnName("recordId");

            entity.HasOne(d => d.Record).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ps_mr");
        });

        modelBuilder.Entity<Prescriptiondetail>(entity =>
        {
            entity.HasKey(e => new { e.PrescriptionId, e.MedicationId }).HasName("pk_psd");

            entity.ToTable("PRESCRIPTIONDETAIL");

            entity.Property(e => e.PrescriptionId).HasColumnName("prescriptionId");
            entity.Property(e => e.MedicationId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("medicationId");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasColumnName("note");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Medication).WithMany(p => p.Prescriptiondetails)
                .HasForeignKey(d => d.MedicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_psd_mdc");

            entity.HasOne(d => d.Prescription).WithMany(p => p.Prescriptiondetails)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_psd_ps");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("pk_sv");

            entity.ToTable("SERVICES");

            entity.Property(e => e.ServiceId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("serviceId");
            entity.Property(e => e.Department)
                .HasMaxLength(200)
                .HasColumnName("department");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.IsCover).HasColumnName("isCover");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(200)
                .HasColumnName("serviceName");
            entity.Property(e => e.SpecialtyId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("specialtyId");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Services)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sv_spc");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.SpecialtyId).HasName("pk_spc");

            entity.ToTable("SPECIALTIES");

            entity.HasIndex(e => e.SpecialtyName, "UQ__SPECIALT__9BDDB60A17245D10").IsUnique();

            entity.Property(e => e.SpecialtyId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("specialtyId");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.SpecialtyName)
                .HasMaxLength(50)
                .HasColumnName("specialtyName");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_us");

            entity.ToTable("USERS", tb => tb.HasTrigger("TG_UPDATE_USER"));

            entity.HasIndex(e => e.Phone, "UQ__USERS__B43B145FF1A13120").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.PassWord)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("passWord");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("P")
                .HasColumnName("role");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("updateAt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
