using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

/// <summary>
/// Tracks patient consents for HIPAA/GDPR compliance, data sharing, and AI-assisted care.
/// </summary>
public class ConsentRecord : TenantEntity
{
    public Guid PatientId { get; set; }
    public ConsentType ConsentType { get; set; }
    public bool IsGranted { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public Guid? WitnessUserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Notes { get; set; }

    // Discrete revocation and actor tracking
    public Guid? GrantedByUserId { get; set; }
    public Guid? RevokedByUserId { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public ApplicationUser? WitnessUser { get; set; }
    public ApplicationUser? GrantedByUser { get; set; }
    public ApplicationUser? RevokedByUser { get; set; }

    public ICollection<ConsentAuditEvent> AuditEvents { get; set; } = new List<ConsentAuditEvent>();
}
