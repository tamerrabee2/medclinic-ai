using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.MedicationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pi => pi.Dose)
            .HasMaxLength(100);

        builder.Property(pi => pi.Frequency)
            .HasMaxLength(100);

        builder.Property(pi => pi.Duration)
            .HasMaxLength(100);

        builder.Property(pi => pi.Route)
            .HasMaxLength(100);

        builder.Property(pi => pi.Instructions)
            .HasMaxLength(1000);

        builder.Property(pi => pi.Notes)
            .HasMaxLength(500);
    }
}
