namespace MedClinic.Application.Models.AI;

public record MedicalImageInput(
    string ImageBase64,
    string ImageType,
    string? BodyPart,
    string? ClinicalIndication,
    string? PatientAge,
    string? PatientGender);

public record MedicalImageAnalysisResult(
    string Summary,
    List<string> Findings,
    List<string> Observations,
    List<string> RegionsOfInterest,
    double? Confidence,
    List<string> RecommendationsForReview,
    bool RequiresDoctorReview = true,
    string Disclaimer = "AI-generated analysis. This content is intended for clinical decision support only and must be reviewed by a qualified healthcare professional.");

public record LabAnalysisInput(
    List<LabValueInput> Values,
    List<LabValueInput>? PreviousValues,
    string? PatientAge,
    string? PatientGender);

public record LabValueInput(
    string TestName,
    string Value,
    string? Unit,
    string? ReferenceRange);

public record LabAnalysisResult(
    string Summary,
    List<LabFinding> Findings,
    List<string> TrendObservations,
    List<string> Recommendations,
    bool RequiresDoctorReview = true);

public record LabFinding(
    string TestName,
    string Value,
    string? Unit,
    string? ReferenceRange,
    string Status,
    string? Trend,
    string? Interpretation);

public record PatientSummaryInput(
    string PatientName,
    int Age,
    string Gender,
    string? ChronicConditions,
    string? Allergies,
    List<string>? RecentDiagnoses,
    List<string>? CurrentMedications,
    List<string>? RecentVisitsSummary);

public record PatientSummaryResult(
    string Summary,
    List<string> KeyFindings,
    List<string> ActiveProblems,
    List<string> Medications,
    List<string> Alerts,
    bool RequiresDoctorReview = true);

public record MedicalReportInput(
    string ReportType,
    string PatientContext,
    string ClinicalData);

public record MedicalReportResult(
    string Report,
    bool RequiresDoctorReview = true);

public record AIChatRequest(
    string Message,
    List<AIChatMessage> History,
    string? PatientContext,
    string? SystemPrompt);

public record AIChatMessage(
    string Role,
    string Content);

public record AIChatResponse(
    string Content,
    int? TokensUsed,
    string Provider);
