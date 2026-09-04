using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.ChangesJson).HasColumnType("jsonb");

        builder.HasIndex(x => new { x.ClinicId, x.UserId });
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.CreatedAt);
        // AuditLog should never be deleted
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
