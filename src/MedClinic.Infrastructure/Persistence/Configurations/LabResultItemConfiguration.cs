using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabResultItemConfiguration : IEntityTypeConfiguration<LabResultItem>
{
    public void Configure(EntityTypeBuilder<LabResultItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TestParameter)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Value)
            .HasMaxLength(200);

        builder.Property(i => i.Unit)
            .HasMaxLength(50);

        builder.Property(i => i.ReferenceRange)
            .HasMaxLength(200);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasOne(i => i.LabResult)
            .WithMany(lr => lr.Items)
            .HasForeignKey(i => i.LabResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
