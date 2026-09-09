using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.ClinicalDecisionSupport;

/// <summary>
/// A deliberately conservative, explainable rule-based baseline for Phase 8.
/// Replace/augment its small curated catalog with a licensed medication knowledge base
/// and locally approved clinical pathways before production clinical use.
/// </summary>
public sealed class RuleBasedClinicalDecisionSupportService : IClinicalDecisionSupportService
{
    private const string Disclaimer = "Clinical decision support only — not a diagnosis or prescribing instruction. A licensed clinician must independently review all findings, patient-specific contraindications, allergies, renal/hepatic function, pregnancy status, doses, and local protocols before clinical use.";
    private readonly ILogger<RuleBasedClinicalDecisionSupportService> _logger;

    private static readonly IReadOnlyDictionary<string, DifferentialRule> DifferentialRules =
        new Dictionary<string, DifferentialRule>(StringComparer.OrdinalIgnoreCase)
        {
            ["chest pain"] = new("Acute coronary syndrome", 0.78m, "Chest pain can represent myocardial ischemia and warrants time-sensitive clinical assessment.", new[] { "Chest pain" }, new[] { "Obtain ECG and serial troponin per local protocol", "Assess for instability and emergency transfer criteria" }),
            ["shortness of breath"] = new("Pulmonary embolism", 0.55m, "Dyspnea has a broad differential; thromboembolic disease is an important condition to consider in appropriate clinical context.", new[] { "Shortness of breath" }, new[] { "Assess oxygen saturation and hemodynamic stability", "Apply an approved pre-test probability pathway" }),
            ["fever"] = new("Acute infection", 0.62m, "Fever may be caused by infectious and non-infectious conditions; source assessment is required.", new[] { "Fever" }, new[] { "Perform focused history and examination", "Order targeted testing guided by suspected source" }),
            ["headache"] = new("Primary headache disorder", 0.48m, "Headache may be primary or secondary; red flags determine urgency.", new[] { "Headache" }, new[] { "Screen for sudden onset, focal deficit, fever, trauma, and altered consciousness", "Escalate urgently if red flags are present" }),
            ["polyuria"] = new("Diabetes mellitus", 0.64m, "Polyuria can be associated with hyperglycemia among other etiologies.", new[] { "Polyuria" }, new[] { "Check point-of-care glucose and HbA1c as clinically appropriate", "Assess hydration and ketone risk when symptomatic" })
        };

    private static readonly IReadOnlyCollection<InteractionRule> InteractionRules = new[]
    {
        new("warfarin", "ibuprofen", InteractionSeverity.Major, "Increased bleeding risk due to anticoagulation plus NSAID-related platelet and gastrointestinal effects.", "Avoid when possible; if a clinician determines co-use is necessary, use the lowest effective exposure and monitor for bleeding and anticoagulation status per protocol.", "Curated Phase 8 baseline rule"),
        new("warfarin", "aspirin", InteractionSeverity.Major, "Increased bleeding risk from combined anticoagulant and antiplatelet effects.", "Do not start, stop, or modify therapy based on this alert alone; require prescriber review and documented indication.", "Curated Phase 8 baseline rule"),
        new("sildenafil", "nitroglycerin", InteractionSeverity.Contraindicated, "Potential for severe hypotension with phosphodiesterase-5 inhibitor and nitrate co-administration.", "Do not co-administer. Initiate urgent clinician review and follow local emergency protocol if exposure occurred.", "Curated Phase 8 baseline rule"),
        new("sildenafil", "isosorbide", InteractionSeverity.Contraindicated, "Potential for severe hypotension with phosphodiesterase-5 inhibitor and nitrate co-administration.", "Do not co-administer. Initiate clinician review before medication administration.", "Curated Phase 8 baseline rule"),
        new("lisinopril", "spironolactone", InteractionSeverity.Major, "Increased hyperkalemia risk, especially with renal impairment.", "Require prescriber review; check potassium and renal function according to local protocol.", "Curated Phase 8 baseline rule"),
        new("sertraline", "tramadol", InteractionSeverity.Major, "Combined serotonergic activity may increase serotonin syndrome and seizure risk.", "Require prescriber/pharmacist review; educate on symptom escalation and consider alternatives where appropriate.", "Curated Phase 8 baseline rule")
    };

    public RuleBasedClinicalDecisionSupportService(ILogger<RuleBasedClinicalDecisionSupportService> logger) => _logger = logger;

    public Task<DifferentialDiagnosisResult> GenerateDifferentialAsync(DifferentialDiagnosisRequest request, CancellationToken ct = default)
    {
        var symptoms = request.Symptoms
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var candidates = symptoms
            .Where(symptom => DifferentialRules.TryGetValue(symptom, out _))
            .Select(symptom => DifferentialRules[symptom].ToCandidate())
            .OrderByDescending(x => x.RelevanceScore)
            .ToList();

        var alerts = BuildRedFlagAlerts(symptoms, request.Vitals);
        _logger.LogInformation("Generated {CandidateCount} differential candidates and {AlertCount} alerts for patient {PatientId}", candidates.Count, alerts.Count, request.PatientId);

        return Task.FromResult(new DifferentialDiagnosisResult(candidates, alerts, Disclaimer, true));
    }

    public Task<DrugInteractionResult> CheckDrugInteractionsAsync(DrugInteractionRequest request, CancellationToken ct = default)
    {
        var medications = request.Medications
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var alerts = InteractionRules
            .Where(rule => medications.Any(m => MatchesMedication(m, rule.MedicationA)) && medications.Any(m => MatchesMedication(m, rule.MedicationB)))
            .Select(rule => new DrugInteractionAlert(rule.MedicationA, rule.MedicationB, rule.Severity, rule.Summary, rule.Management, rule.Source))
            .OrderByDescending(x => x.Severity)
            .ToList();

        _logger.LogInformation("Checked {MedicationCount} medications and found {AlertCount} interactions for patient {PatientId}", medications.Length, alerts.Count, request.PatientId);
        return Task.FromResult(new DrugInteractionResult(alerts, Disclaimer, true));
    }

    private static bool MatchesMedication(string suppliedName, string catalogName) => suppliedName.Contains(catalogName, StringComparison.OrdinalIgnoreCase);

    private static List<ClinicalAlert> BuildRedFlagAlerts(IReadOnlyCollection<string> symptoms, IReadOnlyDictionary<string, string>? vitals)
    {
        var alerts = new List<ClinicalAlert>();
        if (symptoms.Any(s => s.Equals("chest pain", StringComparison.OrdinalIgnoreCase)))
            alerts.Add(new ClinicalAlert(AlertSeverity.Critical, "Chest-pain safety alert", "Potential time-sensitive cardiac and non-cardiac causes must be assessed promptly.", "Apply the clinic's emergency chest-pain pathway and assess for emergency transfer."));
        if (symptoms.Any(s => s.Equals("shortness of breath", StringComparison.OrdinalIgnoreCase)))
            alerts.Add(new ClinicalAlert(AlertSeverity.Critical, "Dyspnea safety alert", "Assess respiratory distress, oxygenation, and hemodynamic stability without delay.", "Apply the clinic's emergency respiratory assessment pathway."));
        if (vitals is not null && vitals.TryGetValue("SpO2", out var spo2) && decimal.TryParse(spo2.TrimEnd('%'), out var value) && value < 90)
            alerts.Add(new ClinicalAlert(AlertSeverity.Critical, "Low oxygen saturation", "Reported SpO2 is below 90%.", "Immediately verify measurement and follow the clinic's emergency escalation protocol."));
        return alerts;
    }

    private sealed record DifferentialRule(string Condition, decimal Score, string Rationale, IReadOnlyCollection<string> Features, IReadOnlyCollection<string> NextSteps)
    {
        public DifferentialDiagnosisCandidate ToCandidate() => new(Condition, Score, Rationale, Features, NextSteps);
    }

    private sealed record InteractionRule(string MedicationA, string MedicationB, InteractionSeverity Severity, string Summary, string Management, string Source);
}
