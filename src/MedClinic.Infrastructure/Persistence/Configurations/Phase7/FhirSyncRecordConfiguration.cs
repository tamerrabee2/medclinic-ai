using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations.Phase7;

public class FhirSyncRecordConfiguration : IEntityTypeConfiguration<FhirSyncRecord>
{
    public void Configure(EntityTypeBuilder<FhirSyncRecord> builder)
    {
        builder.ToTable("FhirSyncRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ResourceType).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ResourceId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ExternalSystem).HasMaxLength(64);
        builder.Property(x => x.PayloadJson).HasDefaultValue("{}");
        builder.Property(x => x.Direction).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
    }
}
