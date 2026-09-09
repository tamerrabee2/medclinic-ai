using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

/// <summary>
/// Audit trail for clinical AI recommendations, ensuring human-in-the-loop review,
/// provenance tracking, model attribution, and medicolegal compliance.
/// </summary>
public class AiDecisionAudit : TenantEntity
{
    public Guid? PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? VisitId { get; set; }

    /// <summary>Clinical capability, e.g. ClinicalSummary, DrugInteraction, LabAnalysis, DifferentialDiagnosis, VoiceScribe</summary>
    public string Capability { get; set; } = string.Empty;

    /// <summary>Underlying provider name: OpenAI, Anthropic, Ollama, Mock</summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>Specific model version string</summary>
    public string? ModelVersion { get; set; }

    /// <summary>Cryptographic hash (SHA256) of the prompt/clinical payload sent to the AI</summary>
    public string? InputHash { get; set; }

    /// <summary>Cryptographic hash (SHA256) of the raw response received from the AI</summary>
    public string? OutputHash { get; set; }

    /// <summary>Model-reported or evaluated confidence score (0.0 to 1.0)</summary>
    public double? ConfidenceScore { get; set; }

    /// <summary>Doctor review status enforcing human-in-the-loop governance</summary>
    public AiReviewStatus ReviewStatus { get; set; } = AiReviewStatus.PendingReview;

    /// <summary>User ID of the physician/clinician who reviewed this decision</summary>
    public Guid? ReviewedByUserId { get; set; }

    /// <summary>Timestamp when the clinical decision was reviewed</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Physician rationale if the AI suggestion was rejected or significantly modified</summary>
    public string? OverrideReason { get; set; }

    /// <summary>Correlation identifier linking related multi-step or agentic calls</summary>
    public string? CorrelationId { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public Visit? Visit { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
}
