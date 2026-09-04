using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.AI;

/// <summary>
/// MockAIProvider — used in Development/Testing environments.
/// Returns realistic structured responses without calling any external API.
/// Switch to OpenAIProvider or AnthropicProvider in Production via appsettings.
/// </summary>
public class MockAIProvider : IAIProvider
{
    private readonly ILogger<MockAIProvider> _logger;
    public MockAIProvider(ILogger<MockAIProvider> logger) => _logger = logger;

    public Task<AIChatResponse> ChatAsync(
        AIChatRequest request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[MockAI] Chat: {Message}", request.UserMessage);

        var responses = new[]
        {
            "Based on the available clinical data, here is a structured assessment:\n\n" +
            "**Key Points:**\n- The presented information suggests further evaluation may be warranted.\n" +
            "- Please review recent lab values and vital signs.\n\n" +
            "⚠️ This is AI-generated content for clinical decision support only.",

            "I've reviewed the patient context. Here are some considerations:\n\n" +
            "1. Monitor vital signs closely\n2. Review medication interactions\n" +
            "3. Consider follow-up in 2 weeks\n\n" +
            "⚠️ This analysis requires physician review before any clinical action.",

            "**Clinical Summary:**\nBased on the available information, the patient's condition " +
            "appears stable. Key areas to monitor include lab trends and symptom progression.\n\n" +
            "⚠️ AI-generated content — must be reviewed by a qualified healthcare professional."
        };

        var response = responses[new Random().Next(responses.Length)];
        return Task.FromResult(new AIChatResponse(response, false));
    }

    public Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[MockAI] Image analysis: {ImageId} ({Modality})",
            input.ImageId, input.Modality);

        var result = new MedicalImageAnalysisResult(
            Summary: $"[DEMO] {input.Modality ?? "Medical"} image analysis complete. " +
                     "No significant abnormalities detected in this demo response.",
            Findings: [
                "[DEMO] Image quality is adequate for assessment.",
                "[DEMO] No acute cardiopulmonary process identified.",
                "[DEMO] Soft tissues appear within normal limits."
            ],
            Observations: [
                "[DEMO] This is a mock AI analysis for development/testing purposes.",
                "[DEMO] In production, a real AI provider will analyze the actual image."
            ],
            RegionsOfInterest: [],
            Confidence: null, // Real confidence only from actual AI providers
            RecommendationsForReview: [
                "Please review with a qualified radiologist.",
                "Correlate with clinical presentation."
            ]
        );

        return Task.FromResult(result);
    }

    public Task<LabAnalysisResult> AnalyzeLabResultsAsync(
        LabAnalysisInput input,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[MockAI] Lab analysis for result: {ResultId}",
            input.CurrentResult.Id);

        var items = input.CurrentResult.Items ?? [];
        var abnormalItems = items.Where(i => !string.IsNullOrEmpty(i.AbnormalFlag)).ToList();

        var values = items.Select(item =>
        {
            var prevResult = input.PreviousResults
                .SelectMany(r => r.Items ?? [])
                .FirstOrDefault(i => i.TestName == item.TestName);

            var status = string.IsNullOrEmpty(item.AbnormalFlag) ? "Normal" :
                         item.AbnormalFlag == "H" ? "High" :
                         item.AbnormalFlag == "L" ? "Low" : "Abnormal";

            string? trend = null;
            if (prevResult != null &&
                double.TryParse(item.Value, out var curr) &&
                double.TryParse(prevResult.Value, out var prev))
            {
                trend = curr > prev * 1.05 ? "Increasing" :
                        curr < prev * 0.95 ? "Decreasing" : "Stable";
            }

            return new LabValueResult(
                item.TestName ?? "Unknown",
                $"{item.Value} {item.Unit}",
                prevResult != null ? $"{prevResult.Value} {prevResult.Unit}" : null,
                item.ReferenceRange,
                status,
                trend
            );
        }).ToList();

        return Task.FromResult(new LabAnalysisResult(
            Summary: $"[DEMO] Analysis of {items.Count} lab parameters. " +
                     $"{abnormalItems.Count} abnormal value(s) detected.",
            Values: values,
            Abnormalities: abnormalItems.Select(i =>
                $"{i.TestName}: {i.Value} {i.Unit} (Ref: {i.ReferenceRange})").ToList(),
            Trends: input.PreviousResults.Any()
                ? ["[DEMO] Trends calculated from previous results."]
                : ["[DEMO] No previous results available for trend comparison."],
            Recommendations: [
                "[DEMO] Review abnormal values with clinical context.",
                "[DEMO] Repeat tests if clinically indicated."
            ]
        ));
    }

    public Task<PatientSummaryResult> SummarizePatientAsync(
        PatientSummaryInput input,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[MockAI] Patient summary for: {PatientId}", input.Patient.Id);

        return Task.FromResult(new PatientSummaryResult(
            Summary: $"[DEMO] Patient {input.Patient.FirstName} {input.Patient.LastName}, " +
                     $"with {input.RecentVisits.Count} recent visit(s). " +
                     "This is a mock summary for development purposes.",
            MedicalHistoryHighlights:
                "[DEMO] Medical history highlights would appear here in production.",
            ActiveConditions: ["[DEMO] Conditions would be extracted from visit diagnoses."],
            CurrentMedications: ["[DEMO] Medications from active prescriptions."],
            RecentAbnormalities: ["[DEMO] Abnormal lab/radiology findings."],
            UpcomingFollowUps: ["[DEMO] Scheduled follow-up appointments."]
        ));
    }

    public Task<MedicalReportResult> GenerateMedicalReportAsync(
        MedicalReportInput input,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[MockAI] Report generation: {Type}", input.ReportType);

        return Task.FromResult(new MedicalReportResult(
            Content: $"[DEMO] {input.ReportType} report generated for development. " +
                     "In production, this will contain a detailed AI-generated report.",
            Format: "markdown"
        ));
    }
}
