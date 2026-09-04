using MedClinic.Application.Models.AI;

namespace MedClinic.Application.Interfaces;

public interface IAIProvider
{
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

    IAsyncEnumerable<string> ChatStreamAsync(
        AIChatRequest request,
        CancellationToken cancellationToken = default);
}
