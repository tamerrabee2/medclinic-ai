using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ChiefComplaint)
            .HasMaxLength(1000);

        builder.Property(v => v.Symptoms)
            .HasMaxLength(2000);

        builder.Property(v => v.Diagnosis)
            .HasMaxLength(2000);

        builder.Property(v => v.DifferentialDiagnosis)
            .HasMaxLength(2000);

        builder.Property(v => v.TreatmentPlan)
            .HasMaxLength(4000);

        builder.Property(v => v.DoctorNotes)
            .HasMaxLength(4000);

        builder.Property(v => v.FollowUpNotes)
            .HasMaxLength(2000);

        builder.HasIndex(v => new { v.ClinicId, v.PatientId });
        builder.HasIndex(v => new { v.ClinicId, v.DoctorId });

        builder.HasOne(v => v.Patient)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Doctor)
            .WithMany()
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Clinic)
            .WithMany()
            .HasForeignKey(v => v.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Appointment)
            .WithOne()
            .HasForeignKey<Visit>(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
