using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityName)
            .HasMaxLength(100);

        builder.Property(al => al.IpAddress)
            .HasMaxLength(50);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        // No sensitive medical data stored in logs
        builder.Property(al => al.Description)
            .HasMaxLength(1000);

        builder.HasIndex(al => new { al.ClinicId, al.UserId });
        builder.HasIndex(al => al.CreatedAt);
        builder.HasIndex(al => al.EntityId);
    }
}
