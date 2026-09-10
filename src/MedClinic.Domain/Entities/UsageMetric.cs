using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class UsageMetric : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public MetricType MetricType { get; set; }

    // Period Bucketing (e.g., Monthly for AI/Storage/DICOM, or Epoch-based for entity counts)
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }

    public long CurrentValue { get; set; } = 0;
    public long QuotaLimit { get; set; } = 0;

    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool HasExceededQuota => QuotaLimit > 0 && CurrentValue >= QuotaLimit;

    public double UsagePercentage => QuotaLimit > 0 ? Math.Min(100.0, (double)CurrentValue / QuotaLimit * 100.0) : 0.0;
}
