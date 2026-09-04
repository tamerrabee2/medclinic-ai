namespace MedClinic.Application.Features.AI.DTOs;

// ── Chat ────────────────────────────────────────────────────────────────────

public record SendMessageRequest(
    Guid?   ConversationId,   // null → create new conversation
    string  Message,
    Guid?   PatientContextId, // optional: inject patient context
    string? AttachmentBase64,
    string? AttachmentMimeType
);

public record AIMessageDto(
    Guid     Id,
    string   Role,           // user | assistant
    string   Content,
    bool     IsStreaming,
    DateTime CreatedAt
);

public record ConversationDto(
    Guid              Id,
    string            Title,
    Guid?             PatientContextId,
    string?           PatientName,
    List<AIMessageDto> Messages,
    DateTime          CreatedAt,
    DateTime          UpdatedAt
);

public record ConversationSummaryDto(
    Guid     Id,
    string   Title,
    string?  LastMessage,
    DateTime UpdatedAt
);

// ── Lab Analyzer ─────────────────────────────────────────────────────────────

public record AnalyzeLabRequest(
    Guid    LabResultId,
    bool    CompareWithPrevious = true
);

public record LabAnalysisResultDto(
    Guid    LabResultId,
    string  Summary,
    List<LabValueAnalysis> Values,
    List<string> Abnormalities,
    List<string> Trends,
    List<string> Recommendations,
    bool    RequiresDoctorReview,
    string  Disclaimer
);

public record LabValueAnalysis(
    string  TestName,
    string  CurrentValue,
    string? PreviousValue,
    string? ReferenceRange,
    string  Status,          // Normal | High | Low | Critical
    string? Trend            // Stable | Increasing | Decreasing
);

// ── Patient Summary ──────────────────────────────────────────────────────────

public record GeneratePatientSummaryRequest(
    Guid PatientId,
    bool IncludeLabTrends    = true,
    bool IncludeRadiology    = true,
    bool IncludeMedications  = true
);

public record PatientSummaryDto(
    Guid    PatientId,
    string  PatientName,
    string  Summary,
    string  MedicalHistoryHighlights,
    List<string> ActiveConditions,
    List<string> CurrentMedications,
    List<string> RecentAbnormalities,
    List<string> UpcomingFollowUps,
    bool    RequiresDoctorReview,
    string  Disclaimer,
    DateTime GeneratedAt
);

// ── Medical Image Analysis ───────────────────────────────────────────────────

public record AnalyzeImageRequest(
    Guid   RadiologyImageId,
    string? ClinicalContext   // optional doctor notes to guide AI
);

public record ImageAnalysisResultDto(
    Guid    RadiologyImageId,
    string  Summary,
    List<string> Findings,
    List<string> Observations,
    List<AIRegionOfInterest> RegionsOfInterest,
    double? Confidence,
    List<string> RecommendationsForReview,
    bool    RequiresDoctorReview,
    string  Disclaimer,
    DateTime GeneratedAt
);

public record AIRegionOfInterest(
    string  Description,
    double? X,
    double? Y,
    double? Width,
    double? Height
);

// ── Review / Approve ─────────────────────────────────────────────────────────

public record ReviewAIResultRequest(
    Guid    AIConversationId,
    string  DoctorNotes,
    bool    Approved,
    string? EditedContent
);
