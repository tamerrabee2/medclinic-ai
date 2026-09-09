using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.ReportedBy)
            .HasMaxLength(200);

        builder.Property(lr => lr.Summary)
            .HasMaxLength(4000);

        builder.HasOne(lr => lr.LabOrder)
            .WithMany(lo => lo.Results)
            .HasForeignKey(lr => lr.LabOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(lr => lr.Items)
            .WithOne(i => i.LabResult)
            .HasForeignKey(i => i.LabResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
