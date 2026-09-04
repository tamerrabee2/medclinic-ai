using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChiefComplaint).HasMaxLength(1000);
        builder.Property(x => x.Symptoms).HasMaxLength(2000);
        builder.Property(x => x.PhysicalExamination).HasMaxLength(4000);
        builder.Property(x => x.Diagnosis).HasMaxLength(2000);
        builder.Property(x => x.DifferentialDiagnosis).HasMaxLength(2000);
        builder.Property(x => x.TreatmentPlan).HasMaxLength(4000);
        builder.Property(x => x.DoctorNotes).HasMaxLength(4000);
        builder.Property(x => x.FollowUpNotes).HasMaxLength(2000);

        // Vitals stored as owned type
        builder.OwnsOne(x => x.Vitals, v =>
        {
            v.Property(x => x.Temperature).HasColumnName("Vitals_Temperature");
            v.Property(x => x.BloodPressureSystolic).HasColumnName("Vitals_BPSystolic");
            v.Property(x => x.BloodPressureDiastolic).HasColumnName("Vitals_BPDiastolic");
            v.Property(x => x.HeartRate).HasColumnName("Vitals_HeartRate");
            v.Property(x => x.RespiratoryRate).HasColumnName("Vitals_RespiratoryRate");
            v.Property(x => x.OxygenSaturation).HasColumnName("Vitals_OxygenSaturation");
            v.Property(x => x.Weight).HasColumnName("Vitals_Weight");
            v.Property(x => x.Height).HasColumnName("Vitals_Height");
            v.Property(x => x.BMI).HasColumnName("Vitals_BMI");
        });

        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
        builder.HasIndex(x => new { x.ClinicId, x.DoctorId });
        builder.HasIndex(x => x.VisitDate);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Visits)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Appointment)
            .WithOne()
            .HasForeignKey<Visit>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
