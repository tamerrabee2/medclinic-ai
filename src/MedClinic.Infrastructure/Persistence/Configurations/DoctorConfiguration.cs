using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Specialty)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.LicenseNumber)
            .HasMaxLength(100);

        builder.HasIndex(d => new { d.ClinicId, d.LicenseNumber })
            .IsUnique()
            .HasFilter("\"LicenseNumber\" IS NOT NULL");

        builder.Property(d => d.Title)
            .HasMaxLength(50);

        builder.Property(d => d.Bio)
            .HasMaxLength(2000);

        builder.HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Clinic)
            .WithMany()
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
