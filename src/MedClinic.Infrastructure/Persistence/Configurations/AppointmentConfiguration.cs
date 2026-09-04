using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Type)
            .HasMaxLength(100);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.CancellationReason)
            .HasMaxLength(500);

        builder.HasIndex(a => new { a.ClinicId, a.DoctorId, a.ScheduledAt });
        builder.HasIndex(a => new { a.ClinicId, a.PatientId });

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Clinic)
            .WithMany()
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
