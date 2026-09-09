using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

/// <summary>
/// AI-powered follow-up recommendation. Doctor must approve before scheduling.
/// </summary>
public class FollowUpIntelligence : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public FollowUpPriority Priority { get; set; } = FollowUpPriority.Routine;
    public int SuggestedDaysFromNow { get; set; }
    public DateTime? SuggestedDate { get; set; }

    /// <summary>JSON array of clinical triggers that generated this recommendation.</summary>
    public string TriggersJson { get; set; } = "[]";

    public bool DoctorApproved { get; set; } = false;
    public bool DoctorDismissed { get; set; } = false;
    public string? DoctorNote { get; set; }
    public DateTime? ActionedAt { get; set; }
    public Guid? ActionedByDoctorId { get; set; }

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

public enum FollowUpPriority
{
    Urgent = 0,
    High = 1,
    Routine = 2,
    Low = 3
}
