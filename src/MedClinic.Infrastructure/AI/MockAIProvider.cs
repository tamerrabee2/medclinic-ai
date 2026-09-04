using MedClinic.Application.Interfaces;
using MedClinic.Application.Models.AI;
using System.Runtime.CompilerServices;

namespace MedClinic.Infrastructure.AI;

/// <summary>
/// Mock AI Provider for development and testing.
/// No API key required. Returns realistic structured demo responses.
/// </summary>
public class MockAIProvider : IAIProvider
{
    public string ProviderName => "Mock";

    public Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input,
        CancellationToken cancellationToken = default)
    {
        var result = new MedicalImageAnalysisResult(
            Summary: $"[DEMO] Analysis of {input.ImageType} image. Body part: {input.BodyPart ?? "Not specified"}. This is a mock response for development purposes.",
            Findings: ["No acute findings detected (demo)", "Image quality is adequate (demo)"],
            Observations: ["Standard anatomical structures visible (demo)"],
            RegionsOfInterest: [],
            Confidence: 0.85,
            RecommendationsForReview: ["Please review by qualified radiologist"],
            RequiresDoctorReview: true);
        return Task.FromResult(result);
    }

    public Task<LabAnalysisResult> AnalyzeLabResultsAsync(
        LabAnalysisInput input,
        CancellationToken cancellationToken = default)
    {
        var findings = input.Values.Select(v => new LabFinding(
            v.TestName, v.Value, v.Unit, v.ReferenceRange,
            Status: "Normal (demo)", Trend: null, Interpretation: "Within expected range (demo)")).ToList();

        var result = new LabAnalysisResult(
            Summary: "[DEMO] Lab results analyzed. All values appear within normal ranges in this demo response.",
            Findings: findings,
            TrendObservations: ["No significant trends detected (demo)"],
            Recommendations: ["Continue routine monitoring"],
            RequiresDoctorReview: true);
        return Task.FromResult(result);
    }

    public Task<PatientSummaryResult> SummarizePatientAsync(
        PatientSummaryInput input,
        CancellationToken cancellationToken = default)
    {
        var result = new PatientSummaryResult(
            Summary: $"[DEMO] Patient summary for {input.PatientName}, {input.Age} years old, {input.Gender}.",
            KeyFindings: ["Demo finding 1", "Demo finding 2"],
            ActiveProblems: input.RecentDiagnoses ?? [],
            Medications: input.CurrentMedications ?? [],
            Alerts: [],
            RequiresDoctorReview: true);
        return Task.FromResult(result);
    }

    public Task<MedicalReportResult> GenerateMedicalReportAsync(
        MedicalReportInput input,
        CancellationToken cancellationToken = default)
    {
        var result = new MedicalReportResult(
            Report: $"[DEMO] {input.ReportType} report generated. This is a mock response for development purposes. In production, connect to a real AI provider.",
            RequiresDoctorReview: true);
        return Task.FromResult(result);
    }

    public Task<AIChatResponse> ChatAsync(
        AIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new AIChatResponse(
            Content: $"[DEMO AI Assistant] I received your message: \"{request.Message}\". " +
                     "This is a mock response. Configure AI_PROVIDER in .env to use a real AI provider (openai, gemini, anthropic, or local).",
            TokensUsed: 42,
            Provider: ProviderName);
        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(
        AIChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var words = $"[DEMO AI] Mock streaming response for: {request.Message}".Split(' ');
        foreach (var word in words)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(50, cancellationToken);
        }
    }
}
