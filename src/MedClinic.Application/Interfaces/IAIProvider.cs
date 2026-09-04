using MedClinic.Domain.Entities;

namespace MedClinic.Application.Interfaces;

// ── Request / Response models ─────────────────────────────────────────────────

public record ChatMessage(string Role, string Content);

public record AIChatRequest(
    string          SystemPrompt,
    List<ChatMessage> History,
    string          UserMessage,
    string?         AttachmentBase64     = null,
    string?         AttachmentMimeType   = null
);

public record AIChatResponse(
    string Content,
    bool   IsStreaming
);

public record MedicalImageInput(
    Guid    ImageId,
    string  ImagePath,
    string? Modality,
    string? ClinicalContext
);

public record MedicalImageAnalysisResult(
    string         Summary,
    List<string>   Findings,
    List<string>   Observations,
    List<RegionOfInterest> RegionsOfInterest,
    double?        Confidence,
    List<string>   RecommendationsForReview
);

public record RegionOfInterest(
    string  Description,
    double? X,
    double? Y,
    double? Width,
    double? Height
);

public record LabAnalysisInput(
    LabResult         CurrentResult,
    List<LabResult>   PreviousResults
);

public record LabAnalysisResult(
    string              Summary,
    List<LabValueResult> Values,
    List<string>        Abnormalities,
    List<string>        Trends,
    List<string>        Recommendations
);

public record LabValueResult(
    string  TestName,
    string  CurrentValue,
    string? PreviousValue,
    string? ReferenceRange,
    string  Status,
    string? Trend
);

public record PatientSummaryInput(
    Patient       Patient,
    List<Visit>   RecentVisits,
    bool          IncludeLabTrends,
    bool          IncludeRadiology,
    bool          IncludeMedications
);

public record PatientSummaryResult(
    string       Summary,
    string       MedicalHistoryHighlights,
    List<string> ActiveConditions,
    List<string> CurrentMedications,
    List<string> RecentAbnormalities,
    List<string> UpcomingFollowUps
);

public record MedicalReportInput(
    string ReportType,
    string Context
);

public record MedicalReportResult(
    string Content,
    string Format
);

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAIProvider
{
    Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input,
        CancellationToken cancellationToken = default);

    Task<LabAnalysisResult> AnalyzeLabResultsAsync(
        LabAnalysisInput input,
        CancellationToken cancellationToken = default);

    Task<PatientSummaryResult> SummarizePatientAsync(
        PatientSummaryInput input,
        CancellationToken cancellationToken = default);

    Task<MedicalReportResult> GenerateMedicalReportAsync(
        MedicalReportInput input,
        CancellationToken cancellationToken = default);

    Task<AIChatResponse> ChatAsync(
        AIChatRequest request,
        CancellationToken cancellationToken = default);
}
