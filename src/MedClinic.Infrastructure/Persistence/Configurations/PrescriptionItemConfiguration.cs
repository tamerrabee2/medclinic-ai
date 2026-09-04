using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MedicationName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Dose).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Frequency).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Duration).HasMaxLength(100);
        builder.Property(x => x.Route).HasMaxLength(100);
        builder.Property(x => x.Instructions).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}
