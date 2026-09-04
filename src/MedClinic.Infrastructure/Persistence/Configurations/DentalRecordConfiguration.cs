using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class DentalRecordConfiguration : IEntityTypeConfiguration<DentalRecord>
{
    public void Configure(EntityTypeBuilder<DentalRecord> b)
    {
        b.HasKey(d => d.Id);
        b.Property(d => d.Condition).IsRequired().HasMaxLength(50);
        b.Property(d => d.Surface).HasMaxLength(100);
        b.Property(d => d.Notes).HasMaxLength(1000);

        // FDI tooth numbers: 11–18, 21–28, 31–38, 41–48
        b.Property(d => d.ToothNumber).IsRequired();

        b.HasIndex(d => new { d.PatientId, d.ToothNumber });
        b.HasIndex(d => new { d.ClinicId, d.PatientId });

        b.HasOne(d => d.Patient)
            .WithMany()
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(d => d.Doctor)
            .WithMany()
            .HasForeignKey(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(d => d.Visit)
            .WithMany()
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
