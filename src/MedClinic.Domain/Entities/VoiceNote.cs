using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class VoiceNote : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? VisitId { get; set; }

    public string AudioFileUrl { get; set; } = string.Empty;
    public long AudioFileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
    public string MimeType { get; set; } = "audio/webm";

    // Transcription
    public string? RawTranscript { get; set; }
    public string? TranscriptLanguage { get; set; }
    public decimal? TranscriptConfidence { get; set; }
    public VoiceNoteStatus Status { get; set; } = VoiceNoteStatus.Pending;

    // Structured Clinical Note (AI-generated, requires doctor review)
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? PhysicalExamination { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? AdditionalNotes { get; set; }

    public bool DoctorApproved { get; set; } = false;
    public DateTime? DoctorApprovedAt { get; set; }
    public Guid? ApprovedByDoctorId { get; set; }

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

public enum VoiceNoteStatus
{
    Pending = 0,
    Transcribing = 1,
    Transcribed = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5
}
