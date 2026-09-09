using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.ClinicalDecisionSupport;

/// <summary>Explainable, deterministic Phase 8 baseline. It is not a validated clinical prediction model or an autonomous triage/reporting system.</summary>
public sealed class ConservativeClinicalAIService : IAdvancedClinicalAIService
{
    private const string Disclaimer = "Assistive output only. A licensed clinician must independently verify this draft and apply local emergency, diagnostic, and treatment protocols.";
    private readonly ILogger<ConservativeClinicalAIService> _logger;
    public ConservativeClinicalAIService(ILogger<ConservativeClinicalAIService> logger) => _logger = logger;

    public Task<RiskScoreResult> CalculateRiskScoreAsync(RiskScoreRequest request, CancellationToken ct = default)
    {
        var contributions = new List<RiskContribution>(); var score = 0;
        if (request.Age is >= 75) { score += 2; contributions.Add(new("Age ≥ 75", 2, "Age is a contextual risk factor and requires individualized interpretation.")); }
        if (TryGetDecimal(request.Vitals, "SpO2", out var spo2) && spo2 < 90) { score += 5; contributions.Add(new("SpO2 < 90%", 5, "Reported hypoxemia requires immediate measurement verification and escalation assessment.")); }
        if (TryGetDecimal(request.Vitals, "SystolicBP", out var systolic) && systolic < 90) { score += 5; contributions.Add(new("Systolic BP < 90", 5, "Reported hypotension can indicate acute instability.")); }
        if (TryGetDecimal(request.Vitals, "HeartRate", out var heartRate) && heartRate > 120) { score += 2; contributions.Add(new("Heart rate > 120", 2, "Tachycardia needs clinical contextualization.")); }
        if (IsAffirmative(request.Factors, "immunosuppressed")) { score += 2; contributions.Add(new("Immunosuppression", 2, "Potentially increased vulnerability; review the complete history.")); }
        var band = score switch { >= 7 => RiskBand.Critical, >= 4 => RiskBand.High, >= 2 => RiskBand.Moderate, _ => RiskBand.Low };
        _logger.LogInformation("Generated conservative risk score {Score} ({Band}) for patient {PatientId}", score, band, request.PatientId);
        return Task.FromResult(new RiskScoreResult("phase8-conservative-v1", score, band, contributions, Disclaimer, true));
    }

    public Task<SoapNoteDraft> SummarizeSoapNoteAsync(SoapNoteRequest request, CancellationToken ct = default)
    {
        var note = request.FreeTextNote?.Trim(); if (string.IsNullOrWhiteSpace(note)) throw new ArgumentException("A free-text note is required.", nameof(request));
        var subjective = ExtractSection(note, "Subjective") ?? ExtractSection(note, "S:") ?? note;
        var objective = ExtractSection(note, "Objective") ?? ExtractSection(note, "O:") ?? "Not documented in source note.";
        var assessment = ExtractSection(note, "Assessment") ?? ExtractSection(note, "A:") ?? "Draft only — clinician assessment required.";
        var plan = ExtractSection(note, "Plan") ?? ExtractSection(note, "P:") ?? "Draft only — clinician must document and approve the plan.";
        _logger.LogInformation("Generated SOAP draft for patient {PatientId}", request.PatientId);
        return Task.FromResult(new SoapNoteDraft(subjective, objective, assessment, plan, Disclaimer, true));
    }

    public Task<TriageAssessment> AssessTriageAsync(TriageRequest request, CancellationToken ct = default)
    {
        var symptoms = request.Symptoms.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase); var reasons = new List<string>(); var level = TriageLevel.Routine;
        if (symptoms.Contains("chest pain") || symptoms.Contains("shortness of breath") || symptoms.Contains("altered consciousness")) { level = TriageLevel.Emergency; reasons.Add("Reported symptom requires immediate clinical assessment for potentially time-sensitive causes."); }
        if (TryGetDecimal(request.Vitals, "SpO2", out var spo2) && spo2 < 90) { level = TriageLevel.Emergency; reasons.Add("Reported SpO2 below 90%; verify immediately and follow emergency escalation protocol."); }
        if (level != TriageLevel.Emergency && (symptoms.Contains("fever") || symptoms.Contains("vomiting") || symptoms.Contains("severe pain"))) { level = TriageLevel.Urgent; reasons.Add("Reported symptom may need same-day clinician assessment depending on the full presentation."); }
        if (reasons.Count == 0) reasons.Add("No baseline red-flag rule was triggered; this does not establish clinical safety.");
        var disposition = level switch { TriageLevel.Emergency => "Immediately apply the clinic emergency escalation pathway; do not rely on this tool as a disposition decision.", TriageLevel.Urgent => "Obtain licensed clinician review using the clinic's same-day triage policy.", _ => "Route for clinician review under the clinic's standard triage policy." };
        _logger.LogInformation("Generated {TriageLevel} triage assessment for patient {PatientId}", level, request.PatientId);
        return Task.FromResult(new TriageAssessment(level, reasons, disposition, Disclaimer, true));
    }

    public Task<RadiologyReportDraft> GenerateRadiologyReportDraftAsync(RadiologyReportRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Findings)) throw new ArgumentException("Findings are required.", nameof(request));
        var title = $"{request.Modality.Trim().ToUpperInvariant()} {request.BodyPart?.Trim()}".Trim(); var technique = $"{request.Modality.Trim().ToUpperInvariant()} examination" + (string.IsNullOrWhiteSpace(request.BodyPart) ? "." : $" of the {request.BodyPart.Trim()}.");
        var impression = request.Impressions?.Any() == true ? string.Join(Environment.NewLine, request.Impressions.Select((x, i) => $"{i + 1}. {x}")) : "No impression supplied. Radiologist interpretation and sign-off required.";
        _logger.LogInformation("Generated radiology report draft for patient {PatientId}; modality {Modality}", request.PatientId, request.Modality);
        return Task.FromResult(new RadiologyReportDraft(title, technique, request.Findings.Trim(), impression, "Draft generated from supplied findings only. It is not a finalized radiology report and requires radiologist review, editing, and sign-off.", true));
    }
    private static bool TryGetDecimal(IReadOnlyDictionary<string, string>? values, string key, out decimal value) { value = 0; return values is not null && values.TryGetValue(key, out var raw) && decimal.TryParse(raw.Trim().TrimEnd('%'), out value); }
    private static bool IsAffirmative(IReadOnlyDictionary<string, string>? values, string key) => values is not null && values.TryGetValue(key, out var raw) && (raw.Equals("true", StringComparison.OrdinalIgnoreCase) || raw.Equals("yes", StringComparison.OrdinalIgnoreCase));
    private static string? ExtractSection(string note, string marker) { var index = note.IndexOf(marker, StringComparison.OrdinalIgnoreCase); if (index < 0) return null; var remainder = note[(index + marker.Length)..].TrimStart(':', ' ', '\r', '\n'); var next = new[] { "Subjective", "Objective", "Assessment", "Plan", "S:", "O:", "A:", "P:" }.Select(m => remainder.IndexOf(m, StringComparison.OrdinalIgnoreCase)).Where(i => i >= 0).DefaultIfEmpty(-1).Min(); return (next >= 0 ? remainder[..next] : remainder).Trim(); }
}
