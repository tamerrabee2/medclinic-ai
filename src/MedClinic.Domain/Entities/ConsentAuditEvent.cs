using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

/// <summary>
/// Immutable audit event tracking each lifecycle transition (Granted, Revoked, Expired, Verified)
/// of a patient's consent record for regulatory and HIPAA/GDPR compliance.
/// </summary>
public class ConsentAuditEvent : TenantEntity
{
    public Guid ConsentRecordId { get; set; }
    public Guid PatientId { get; set; }
    public ConsentAuditEventType EventType { get; set; }
    public ConsentType ConsentType { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ConsentRecord ConsentRecord { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ApplicationUser? PerformedByUser { get; set; }
}
