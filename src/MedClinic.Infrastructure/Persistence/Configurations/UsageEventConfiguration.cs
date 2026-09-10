using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class UsageEventConfiguration : IEntityTypeConfiguration<UsageEvent>
{
    public void Configure(EntityTypeBuilder<UsageEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OperationId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.ReleaseReason)
            .HasMaxLength(512);

        builder.Property(e => e.RequestPayloadHash)
            .HasMaxLength(128);

        // Unique index on (ClinicId, MetricType, IdempotencyKey) to guarantee idempotency per metric & tenant
        builder.HasIndex(e => new { e.ClinicId, e.MetricType, e.IdempotencyKey })
            .IsUnique();

        // Index for querying active reservations during quota checks
        builder.HasIndex(e => new { e.ClinicId, e.MetricType, e.Status, e.PeriodStartUtc, e.ExpiresAtUtc });

        builder.HasOne(e => e.Clinic)
            .WithMany(c => c.UsageEvents)
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
