using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class RadiologyStudy : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? VisitId { get; set; }
    public string StudyType { get; set; } = string.Empty; // X-Ray, CT, MRI, Ultrasound
    public string? BodyPart { get; set; }
    public DateTime StudyDate { get; set; } = DateTime.UtcNow;
    public string? ClinicalInfo { get; set; }
    public string? Findings { get; set; }
    public string? Impression { get; set; }
    public string? Notes { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime? ReportedAt { get; set; }
    public string? AccessionNumber { get; set; }
    public RadiologyStudyStatus Status { get; set; } = RadiologyStudyStatus.Pending;
    public bool IsAIAnalyzed { get; set; } = false;
    public bool DoctorReviewed { get; set; } = false;

    public Patient Patient { get; set; } = null!;
    public ICollection<MedicalImage> Images { get; set; } = [];
    public ICollection<AIAnalysis> AIAnalyses { get; set; } = [];
}

public enum RadiologyStudyStatus { Pending, InProgress, Completed, Cancelled }
