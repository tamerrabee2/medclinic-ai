using MedClinic.Domain.Enums;

namespace MedClinic.Application.Features.AI.DTOs;

public class RecordAiDecisionRequest
{
    public Guid? PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? VisitId { get; set; }
    public string Capability { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? ModelVersion { get; set; }
    public string InputPayload { get; set; } = string.Empty;
    public string OutputPayload { get; set; } = string.Empty;
    public double? ConfidenceScore { get; set; }
    public string? CorrelationId { get; set; }
}

public class ReviewAiDecisionRequest
{
    public AiReviewStatus Status { get; set; }
    public string? OverrideReason { get; set; }
}

public class AiDecisionAuditDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public string? PatientName { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public Guid? VisitId { get; set; }
    public string Capability { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? ModelVersion { get; set; }
    public string? InputHash { get; set; }
    public string? OutputHash { get; set; }
    public double? ConfidenceScore { get; set; }
    public AiReviewStatus ReviewStatus { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? OverrideReason { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AiAuditFilterRequest
{
    public AiReviewStatus? Status { get; set; }
    public string? Capability { get; set; }
    public Guid? PatientId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
