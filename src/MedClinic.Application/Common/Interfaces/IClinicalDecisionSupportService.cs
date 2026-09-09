namespace MedClinic.Application.Common.Interfaces;

/// <summary>
/// Clinical decision support is assistive only. It must never make a final diagnosis,
/// prescribe therapy, or replace the independent judgment of a licensed clinician.
/// </summary>
public interface IClinicalDecisionSupportService
{
    Task<DifferentialDiagnosisResult> GenerateDifferentialAsync(
        DifferentialDiagnosisRequest request,
        CancellationToken ct = default);

    Task<DrugInteractionResult> CheckDrugInteractionsAsync(
        DrugInteractionRequest request,
        CancellationToken ct = default);
}

public record DifferentialDiagnosisRequest(
    Guid PatientId,
    IReadOnlyCollection<string> Symptoms,
    IReadOnlyDictionary<string, string>? Vitals,
    IReadOnlyDictionary<string, string>? LabResults,
    string? ClinicalNotes);

public record DifferentialDiagnosisResult(
    IReadOnlyCollection<DifferentialDiagnosisCandidate> Candidates,
    IReadOnlyCollection<ClinicalAlert> Alerts,
    string Disclaimer,
    bool RequiresClinicianReview);

public record DifferentialDiagnosisCandidate(
    string Condition,
    decimal RelevanceScore,
    string Rationale,
    IReadOnlyCollection<string> SupportingFeatures,
    IReadOnlyCollection<string> SuggestedNextSteps);

public record DrugInteractionRequest(
    Guid PatientId,
    IReadOnlyCollection<MedicationInput> Medications);

public record MedicationInput(string Name, string? Dose = null, string? Route = null);

public record DrugInteractionResult(
    IReadOnlyCollection<DrugInteractionAlert> Interactions,
    string Disclaimer,
    bool RequiresClinicianReview);

public record DrugInteractionAlert(
    string MedicationA,
    string MedicationB,
    InteractionSeverity Severity,
    string Summary,
    string ClinicalManagement,
    string EvidenceSource);

public record ClinicalAlert(
    AlertSeverity Severity,
    string Title,
    string Message,
    string SuggestedAction);

public enum InteractionSeverity { Minor = 0, Moderate = 1, Major = 2, Contraindicated = 3 }
public enum AlertSeverity { Information = 0, Warning = 1, Critical = 2 }
