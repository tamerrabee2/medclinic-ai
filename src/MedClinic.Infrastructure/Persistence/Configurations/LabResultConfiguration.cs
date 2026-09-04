using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.TestName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(lr => lr.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(lr => lr.Notes)
            .HasMaxLength(2000);

        builder.Property(lr => lr.AiSummary)
            .HasMaxLength(4000);

        builder.HasMany(lr => lr.Items)
            .WithOne(i => i.LabResult)
            .HasForeignKey(i => i.LabResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
