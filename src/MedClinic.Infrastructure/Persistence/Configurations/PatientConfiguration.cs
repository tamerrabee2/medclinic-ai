using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.HasKey(p => p.Id);
        b.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        b.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        b.Property(p => p.Phone).HasMaxLength(30);
        b.Property(p => p.Email).HasMaxLength(200);
        b.Property(p => p.NationalId).HasMaxLength(50);
        b.Property(p => p.FileNumber).HasMaxLength(50);
        b.HasIndex(p => new { p.ClinicId, p.FileNumber }).IsUnique().HasFilter("\"FileNumber\" IS NOT NULL");
        b.HasIndex(p => new { p.ClinicId, p.NationalId }).IsUnique().HasFilter("\"NationalId\" IS NOT NULL");
        b.HasIndex(p => new { p.ClinicId, p.Phone });

        // Relationships
        b.HasMany(p => p.Allergies)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
