using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class UsageMetricConfiguration : IEntityTypeConfiguration<UsageMetric>
{
    public void Configure(EntityTypeBuilder<UsageMetric> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasIndex(m => new { m.ClinicId, m.MetricType, m.PeriodStartUtc });
        builder.HasIndex(m => new { m.ClinicId, m.PeriodEndUtc });

        builder.HasOne(m => m.Clinic)
            .WithMany(c => c.UsageMetrics)
            .HasForeignKey(m => m.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
