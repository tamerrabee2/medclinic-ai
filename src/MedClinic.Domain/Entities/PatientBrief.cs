using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

/// <summary>
/// AI-generated patient brief — always requires doctor review before use.
/// </summary>
public class PatientBrief : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }

    /// <summary>AI-generated narrative summary of patient status.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>JSON array of recent significant changes (lab, vitals, medications).</summary>
    public string RecentChangesJson { get; set; } = "[]";

    /// <summary>JSON array of pending items (labs, imaging, follow-ups).</summary>
    public string PendingItemsJson { get; set; } = "[]";

    /// <summary>JSON array of AI-flagged alerts or concerns.</summary>
    public string AlertsJson { get; set; } = "[]";

    public string AIModel { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public bool RequiresDoctorReview { get; set; } = true;
    public bool DoctorReviewed { get; set; } = false;
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
