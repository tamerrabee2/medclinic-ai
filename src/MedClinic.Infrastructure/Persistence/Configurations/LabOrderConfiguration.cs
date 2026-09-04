using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.HasKey(lo => lo.Id);

        builder.Property(lo => lo.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(lo => lo.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(lo => new { lo.ClinicId, lo.PatientId });

        builder.HasOne(lo => lo.Patient)
            .WithMany()
            .HasForeignKey(lo => lo.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lo => lo.Doctor)
            .WithMany()
            .HasForeignKey(lo => lo.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(lo => lo.Results)
            .WithOne(r => r.LabOrder)
            .HasForeignKey(r => r.LabOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
