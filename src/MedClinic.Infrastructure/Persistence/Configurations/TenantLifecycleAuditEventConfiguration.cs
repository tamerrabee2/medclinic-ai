using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class TenantLifecycleAuditEventConfiguration : IEntityTypeConfiguration<TenantLifecycleAuditEvent>
{
    public void Configure(EntityTypeBuilder<TenantLifecycleAuditEvent> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(64);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.Property(a => a.PerformedByUserName)
            .HasMaxLength(200);

        builder.Property(a => a.OldValue)
            .HasMaxLength(2000);

        builder.Property(a => a.NewValue)
            .HasMaxLength(2000);

        builder.HasIndex(a => new { a.ClinicId, a.Timestamp });
        builder.HasIndex(a => new { a.ClinicId, a.EventType });
        builder.HasIndex(a => a.EventType);

        builder.HasOne(a => a.Clinic)
            .WithMany(c => c.LifecycleEvents)
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
