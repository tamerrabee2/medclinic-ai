using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

/// <summary>
/// Represents a single event on the patient's medical timeline.
/// </summary>
public class ClinicalTimelineEvent : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }

    public TimelineEventType EventType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }

    /// <summary>Reference to the source entity (VisitId, LabOrderId, etc.)</summary>
    public Guid? SourceEntityId { get; set; }
    public string? SourceEntityType { get; set; }

    /// <summary>AI-generated significance note for this event.</summary>
    public string? AISignificance { get; set; }
    public bool IsAIHighlighted { get; set; } = false;

    // JSON metadata (flexible per event type)
    public string MetadataJson { get; set; } = "{}";

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

public enum TimelineEventType
{
    Visit = 0,
    Diagnosis = 1,
    Prescription = 2,
    LabOrder = 3,
    LabResult = 4,
    RadiologyStudy = 5,
    ImagingAnalysis = 6,
    VoiceNote = 7,
    PatientBrief = 8,
    Appointment = 9,
    Procedure = 10,
    Alert = 11,
    FollowUp = 12
}
