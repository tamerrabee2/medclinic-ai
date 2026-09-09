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

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public ApplicationUser? WitnessUser { get; set; }
}
