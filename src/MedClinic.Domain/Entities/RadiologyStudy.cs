using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class RadiologyStudy : TenantEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? VisitId { get; set; }
    public Visit? Visit { get; set; }
    public string StudyType { get; set; } = string.Empty;
    public string? BodyPart { get; set; }
    public string? ClinicalIndication { get; set; }
    public string? Report { get; set; }
    public bool IsAIAnalyzed { get; set; } = false;
    public bool RequiresDoctorReview { get; set; } = true;
    public DateTime StudyDate { get; set; } = DateTime.UtcNow;
    public Clinic Clinic { get; set; } = null!;

    public ICollection<MedicalImage> Images { get; set; } = [];
    public ICollection<AIAnalysis> AIAnalyses { get; set; } = [];
}
