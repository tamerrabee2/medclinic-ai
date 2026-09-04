using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabResultItemConfiguration : IEntityTypeConfiguration<LabResultItem>
{
    public void Configure(EntityTypeBuilder<LabResultItem> builder)
    {
        builder.ToTable("LabResultItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TestName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Value).HasMaxLength(200);
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.ReferenceRange).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}
