using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.MedicineName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pi => pi.Dosage)
            .HasMaxLength(100);

        builder.Property(pi => pi.Frequency)
            .HasMaxLength(100);

        builder.Property(pi => pi.Route)
            .HasMaxLength(100);

        builder.Property(pi => pi.Instructions)
            .HasMaxLength(1000);

        builder.Property(pi => pi.Notes)
            .HasMaxLength(500);

        builder.Ignore(pi => pi.MedicationName);
        builder.Ignore(pi => pi.Dose);
        builder.Ignore(pi => pi.Duration);
    }
}
