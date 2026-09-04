using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIAnalysis : TenantEntity
{
    public Guid? PatientId { get; set; }
    public Guid? MedicalImageId { get; set; }
    public Guid? LabResultId { get; set; }
    public Guid? VisitId { get; set; }
    public Guid RequestedByDoctorId { get; set; }
    public Guid? ReviewedByDoctorId { get; set; }
    public string AnalysisType { get; set; } = string.Empty; // MedicalImage, LabResult, PatientSummary, MedicalReport
    public string Status { get; set; } = AIAnalysisStatus.Pending;
    public string? Summary { get; set; }
    public string? ResultJson { get; set; } // Structured JSON result from AI
    public string? ErrorMessage { get; set; }
    public string? AIProvider { get; set; }
    public string? ModelVersion { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool DoctorApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public string? DoctorComments { get; set; }
    public string Disclaimer { get; set; } = "AI-generated analysis. This content is intended for clinical decision support only and must be reviewed by a qualified healthcare professional.";

    public Patient? Patient { get; set; }
    public MedicalImage? MedicalImage { get; set; }
    public Doctor RequestedByDoctor { get; set; } = null!;
    public Doctor? ReviewedByDoctor { get; set; }
}

public static class AIAnalysisStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string ReviewRequired = "ReviewRequired";
    public const string Approved = "Approved";
}
