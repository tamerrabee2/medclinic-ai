using MedClinic.Application.Models.AI;

namespace MedClinic.Application.Interfaces;

/// <summary>
/// Abstraction for AI providers. Swap OpenAI, Gemini, Anthropic, or Local AI
/// without touching Business Logic.
/// </summary>
public interface IAIProvider
{
    /// <summary>Name/identifier of this provider (e.g., "Mock", "OpenAI", "Gemini")</summary>
    string ProviderName { get; }

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
