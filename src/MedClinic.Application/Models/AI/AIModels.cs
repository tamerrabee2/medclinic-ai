namespace MedClinic.Application.Models.AI;

// ── Medical Image Analysis ───────────────────────────────────────────────────────
public record MedicalImageInput(
    Stream ImageStream,
    string FileName,
    string ContentType,
    string? Modality,
    string? BodyPart,
    string? ClinicalContext);

public record MedicalImageAnalysisResult(
    string Summary,
    List<string> Findings,
    List<string> Observations,
    List<RegionOfInterest> RegionsOfInterest,
    double? Confidence,
    List<string> RecommendationsForReview,
    bool RequiresDoctorReview = true,
    string Disclaimer = "AI-generated analysis. This content is intended for clinical decision support only and must be reviewed by a qualified healthcare professional.");

public record RegionOfInterest(
    string Label,
    string? Description,
    BoundingBox? BoundingBox);

public record BoundingBox(double X, double Y, double Width, double Height);

// ── Lab Analysis ────────────────────────────────────────────────────────────────────
public record LabAnalysisInput(
    List<LabTestInput> Tests,
    List<LabTestInput>? PreviousTests,
    string? PatientAge,
    string? PatientGender,
    string? ClinicalContext);

public record LabTestInput(
    string TestName,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    string? PreviousValue);

public record LabAnalysisResult(
    string Summary,
    List<LabTestAnalysis> Tests,
    List<string> AbnormalFindings,
    List<string> TrendObservations,
    List<string> Recommendations,
    bool RequiresDoctorReview = true,
    string Disclaimer = "AI-generated analysis. This content is intended for clinical decision support only and must be reviewed by a qualified healthcare professional.");

public record LabTestAnalysis(
    string TestName,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    string Status,
    string? TrendDirection,
    string? Interpretation);

// ── Patient Summary ──────────────────────────────────────────────────────────────────npublic record PatientSummaryInput(
    string PatientName,
    string? Age,
    string? Gender,
    string? ChronicConditions,
    string? Allergies,
    List<string> RecentDiagnoses,
    List<string> CurrentMedications,
    List<string> RecentLabSummaries,
    List<string> RecentVisitSummaries,
    string? AdditionalContext);

public record PatientSummaryResult(
    string Summary,
    List<string> KeyFindings,
    List<string> ActiveProblems,
    List<string> ImportantAlerts,
    List<string> SuggestedFollowUp,
    bool RequiresDoctorReview = true,
    string Disclaimer = "AI-generated summary. Must be reviewed by a qualified healthcare professional.");

// ── Medical Report ────────────────────────────────────────────────────────────────────
public record MedicalReportInput(
    string ReportType,
    string PatientName,
    string? Age,
    string? ClinicalContext,
    List<string> DataPoints,
    string? AdditionalInstructions);

public record MedicalReportResult(
    string Title,
    string Content,
    List<string> Sections,
    bool RequiresDoctorReview = true,
    string Disclaimer = "AI-generated report. Must be reviewed and approved by a qualified healthcare professional.");

// ── AI Chat ─────────────────────────────────────────────────────────────────────────
public record AIChatRequest(
    List<ChatMessage> Messages,
    string? SystemPrompt,
    string? PatientContext,
    bool Stream = false);

public record ChatMessage(string Role, string Content);

public record AIChatResponse(
    string Content,
    string? Role = "assistant",
    int? TokensUsed = null,
    bool IsError = false,
    string? ErrorMessage = null);
