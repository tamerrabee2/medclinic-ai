using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class TenantLifecycleAuditEvent : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public TenantLifecycleEventType EventType { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }

    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}
