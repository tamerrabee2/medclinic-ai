namespace MedClinic.Application.Common.Interfaces;

/// <summary>
/// Assistive clinical AI only. Each response requires independent clinician/radiologist review.
/// No output from this contract may autonomously diagnose, triage, prescribe, or finalize a report.
/// </summary>
public interface IAdvancedClinicalAIService
{
    Task<RiskScoreResult> CalculateRiskScoreAsync(RiskScoreRequest request, CancellationToken ct = default);
    Task<SoapNoteDraft> SummarizeSoapNoteAsync(SoapNoteRequest request, CancellationToken ct = default);
    Task<TriageAssessment> AssessTriageAsync(TriageRequest request, CancellationToken ct = default);
    Task<RadiologyReportDraft> GenerateRadiologyReportDraftAsync(RadiologyReportRequest request, CancellationToken ct = default);
}

public record RiskScoreRequest(Guid PatientId, int? Age, IReadOnlyDictionary<string, string>? Vitals, IReadOnlyDictionary<string, string>? Factors);
public record RiskScoreResult(string ModelVersion, int Score, RiskBand Band, IReadOnlyCollection<RiskContribution> Contributions, string Disclaimer, bool RequiresClinicianReview);
public record RiskContribution(string Factor, int Points, string Rationale);
public enum RiskBand { Low = 0, Moderate = 1, High = 2, Critical = 3 }
public record SoapNoteRequest(Guid PatientId, string FreeTextNote, string? VisitContext = null);
public record SoapNoteDraft(string Subjective, string Objective, string Assessment, string Plan, string Disclaimer, bool RequiresClinicianReview);
public record TriageRequest(Guid PatientId, IReadOnlyCollection<string> Symptoms, IReadOnlyDictionary<string, string>? Vitals, string? Notes = null);
public record TriageAssessment(TriageLevel Level, IReadOnlyCollection<string> Reasons, string SuggestedDisposition, string Disclaimer, bool RequiresClinicianReview);
public enum TriageLevel { Routine = 0, Urgent = 1, Emergency = 2 }
public record RadiologyReportRequest(Guid PatientId, string Modality, string? BodyPart, string Findings, IReadOnlyCollection<string>? Impressions = null);
public record RadiologyReportDraft(string Title, string Technique, string Findings, string Impression, string Disclaimer, bool RequiresRadiologistReview);
