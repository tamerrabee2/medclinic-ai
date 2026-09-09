using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations.Phase7;

public class ExternalLabProviderConfiguration : IEntityTypeConfiguration<ExternalLabProvider>
{
    public void Configure(EntityTypeBuilder<ExternalLabProvider> builder)
    {
        builder.ToTable("ExternalLabProviders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ProviderCode).IsRequired().HasMaxLength(64);
        builder.Property(x => x.AuthType).HasMaxLength(64);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ExternalLabSyncConfiguration : IEntityTypeConfiguration<ExternalLabSync>
{
    public void Configure(EntityTypeBuilder<ExternalLabSync> builder)
    {
        builder.ToTable("ExternalLabSyncs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalOrderId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(64);
        builder.Property(x => x.PayloadJson).HasDefaultValue("{}");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
    }
}
