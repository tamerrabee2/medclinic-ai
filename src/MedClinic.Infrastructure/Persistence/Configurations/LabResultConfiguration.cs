using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.ToTable("LabResults");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.FileUrl).HasMaxLength(500);
        builder.Property(x => x.PerformedBy).HasMaxLength(200);

        builder.HasIndex(x => x.LabOrderId);
        builder.HasIndex(x => x.ResultDate);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.LabResult)
            .HasForeignKey(x => x.LabResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
