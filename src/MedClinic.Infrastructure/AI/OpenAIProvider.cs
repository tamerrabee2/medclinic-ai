using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MedClinic.Infrastructure.AI;

/// <summary>
/// OpenAI provider — requires AI:OpenAI:ApiKey in configuration.
/// Supports GPT-4o and GPT-4o-mini.
/// </summary>
public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly string     _model;
    private readonly int        _maxTokens;
    private readonly ILogger<OpenAIProvider> _logger;

    public OpenAIProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<OpenAIProvider> logger)
    {
        _http      = httpClientFactory.CreateClient("OpenAI");
        _model     = config["AI:OpenAI:Model"] ?? "gpt-4o";
        _maxTokens = int.Parse(config["AI:OpenAI:MaxTokens"] ?? "2048");
        _logger    = logger;

        var apiKey = config["AI:OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<AIChatResponse> ChatAsync(
        AIChatRequest request, CancellationToken ct = default)
    {
        var messages = new List<object>
        {
            new { role = "system", content = request.SystemPrompt }
        };

        foreach (var h in request.History)
            messages.Add(new { role = h.Role, content = h.Content });

        // Support image attachments
        if (!string.IsNullOrEmpty(request.AttachmentBase64))
        {
            messages.Add(new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = request.UserMessage },
                    new { type = "image_url", image_url = new
                    {
                        url = $"data:{request.AttachmentMimeType};base64,{request.AttachmentBase64}"
                    }}
                }
            });
        }
        else
        {
            messages.Add(new { role = "user", content = request.UserMessage });
        }

        var body = new
        {
            model = _model,
            max_tokens = _maxTokens,
            messages
        };

        var json     = JsonSerializer.Serialize(body);
        var content  = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/v1/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc    = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        return new AIChatResponse(text, false);
    }

    public async Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input, CancellationToken ct = default)
    {
        var prompt =
            $"You are a radiology AI assistant. Analyze this {input.Modality ?? "medical"} image. " +
            (input.ClinicalContext != null
                ? $"Clinical context: {input.ClinicalContext}. "
                : "") +
            "Provide a structured analysis with: summary, findings (list), observations (list), " +
            "and any regions of interest if visible. " +
            "IMPORTANT: This is for clinical decision support only. " +
            "Always state that physician review is required.";

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Please analyze this medical image."
        );

        // Note: actual image bytes would be passed from storage in production
        var chatResponse = await ChatAsync(request, ct);

        return new MedicalImageAnalysisResult(
            Summary: chatResponse.Content,
            Findings: [],
            Observations: [],
            RegionsOfInterest: [],
            Confidence: null,
            RecommendationsForReview: ["Physician review required before clinical action."]
        );
    }

    public async Task<LabAnalysisResult> AnalyzeLabResultsAsync(
        LabAnalysisInput input, CancellationToken ct = default)
    {
        var items = input.CurrentResult.Items ?? [];
        var labData = string.Join("\n",
            items.Select(i =>
                $"- {i.TestName}: {i.Value} {i.Unit} " +
                $"(Ref: {i.ReferenceRange}, Flag: {i.AbnormalFlag ?? "Normal"})"));

        var prompt = "You are a clinical laboratory AI assistant. " +
            "Analyze the following lab results and provide:\n" +
            "1. A brief summary\n" +
            "2. List of abnormal values with clinical significance\n" +
            "3. Trend analysis (if previous results provided)\n" +
            "4. Clinical recommendations\n" +
            "Always note that physician review is mandatory.\n\n" +
            $"Lab Results:\n{labData}";

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Please analyze these lab results."
        );

        var chatResponse = await ChatAsync(request, ct);

        return new LabAnalysisResult(
            Summary: chatResponse.Content,
            Values: items.Select(i => new LabValueResult(
                i.TestName ?? "Unknown",
                $"{i.Value} {i.Unit}",
                null, i.ReferenceRange,
                string.IsNullOrEmpty(i.AbnormalFlag) ? "Normal" : i.AbnormalFlag!,
                null)).ToList(),
            Abnormalities: items
                .Where(i => !string.IsNullOrEmpty(i.AbnormalFlag))
                .Select(i => $"{i.TestName}: {i.Value} {i.Unit}")
                .ToList(),
            Trends: [],
            Recommendations: ["Physician review required."]
        );
    }

    public async Task<PatientSummaryResult> SummarizePatientAsync(
        PatientSummaryInput input, CancellationToken ct = default)
    {
        var visits  = string.Join("\n", input.RecentVisits
            .Select(v => $"- {v.VisitDate:yyyy-MM-dd}: {v.ChiefComplaint}"));

        var prompt =
            $"Summarize the medical history for patient: {input.Patient.FirstName} {input.Patient.LastName}, " +
            $"Age: {DateTime.Today.Year - (input.Patient.DateOfBirth?.Year ?? 0)}, " +
            $"Gender: {input.Patient.Gender}.\n\n" +
            $"Recent visits:\n{(string.IsNullOrEmpty(visits) ? "None recorded" : visits)}\n\n" +
            "Provide: summary, active conditions, current medications, recent abnormalities, follow-up needs.";

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Please generate a clinical summary for this patient."
        );

        var chatResponse = await ChatAsync(request, ct);

        return new PatientSummaryResult(
            Summary: chatResponse.Content,
            MedicalHistoryHighlights: "",
            ActiveConditions: [],
            CurrentMedications: [],
            RecentAbnormalities: [],
            UpcomingFollowUps: []
        );
    }

    public async Task<MedicalReportResult> GenerateMedicalReportAsync(
        MedicalReportInput input, CancellationToken ct = default)
    {
        var request = new AIChatRequest(
            SystemPrompt: $"Generate a {input.ReportType} medical report based on the provided context.",
            History: [],
            UserMessage: input.Context
        );
        var response = await ChatAsync(request, ct);
        return new MedicalReportResult(response.Content, "markdown");
    }
}
