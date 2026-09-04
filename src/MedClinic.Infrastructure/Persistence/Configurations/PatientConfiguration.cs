using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.NationalId)
            .HasMaxLength(50);

        builder.HasIndex(p => new { p.ClinicId, p.NationalId })
            .IsUnique()
            .HasFilter("\"NationalId\" IS NOT NULL");

        builder.Property(p => p.Phone)
            .HasMaxLength(50);

        builder.Property(p => p.Email)
            .HasMaxLength(200);

        builder.Property(p => p.Address)
            .HasMaxLength(500);

        builder.Property(p => p.Allergies)
            .HasMaxLength(2000);

        builder.Property(p => p.ChronicConditions)
            .HasMaxLength(2000);

        builder.Property(p => p.Notes)
            .HasMaxLength(4000);

        builder.HasOne(p => p.Clinic)
            .WithMany()
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Visits)
            .WithOne(v => v.Patient)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
