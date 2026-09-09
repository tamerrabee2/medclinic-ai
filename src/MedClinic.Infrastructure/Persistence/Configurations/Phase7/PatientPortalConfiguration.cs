using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations.Phase7;

public class PatientPortalAccessConfiguration : IEntityTypeConfiguration<PatientPortalAccess>
{
    public void Configure(EntityTypeBuilder<PatientPortalAccess> builder)
    {
        builder.ToTable("PatientPortalAccesses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
        builder.HasIndex(x => x.Email);
    }
}

public class PatientPortalMessageConfiguration : IEntityTypeConfiguration<PatientPortalMessage>
{
    public void Configure(EntityTypeBuilder<PatientPortalMessage> builder)
    {
        builder.ToTable("PatientPortalMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.SenderType).IsRequired().HasMaxLength(32);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
    }
}
