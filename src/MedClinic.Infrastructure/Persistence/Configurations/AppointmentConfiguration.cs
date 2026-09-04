using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.HasKey(a => a.Id);
        b.Property(a => a.Status).HasConversion<string>();
        b.Property(a => a.ReasonForVisit).HasMaxLength(500);
        b.Property(a => a.Notes).HasMaxLength(1000);
        b.HasIndex(a => new { a.ClinicId, a.DoctorId, a.ScheduledAt });
        b.HasIndex(a => new { a.ClinicId, a.PatientId });
        b.HasIndex(a => new { a.ClinicId, a.ScheduledAt });

        b.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
