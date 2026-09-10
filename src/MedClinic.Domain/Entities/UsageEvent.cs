using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class UsageEvent : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public MetricType MetricType { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }

    public string OperationId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;

    public long Delta { get; set; } = 1;
    public UsageEventStatus Status { get; set; } = UsageEventStatus.Reserved;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? CommittedAtUtc { get; set; }
    public DateTime? ReleasedAtUtc { get; set; }
    public string? ReleaseReason { get; set; }
}
