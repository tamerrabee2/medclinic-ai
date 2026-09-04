using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIAnalysis : TenantEntity
{
    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }
    public Guid? RadiologyStudyId { get; set; }
    public RadiologyStudy? RadiologyStudy { get; set; }
    public Guid? LabResultId { get; set; }
    public LabResult? LabResult { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? InputSummary { get; set; }
    public string? ResultJson { get; set; }
    public bool RequiresDoctorReview { get; set; } = true;
    public bool IsApproved { get; set; } = false;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Disclaimer { get; set; } = "AI-generated analysis. This content is intended for clinical decision support only and must be reviewed by a qualified healthcare professional.";
    public Clinic Clinic { get; set; } = null!;
}
