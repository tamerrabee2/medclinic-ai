using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MedClinic.Infrastructure.AI;

/// <summary>
/// Local AI Provider — compatible with Ollama, LocalAI, LM Studio (OpenAI-compatible /v1/chat/completions).
/// Configured via AI:Local:BaseUrl (default: http://localhost:11434) and AI:Local:Model (default: llama3).
/// </summary>
public class LocalAIProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _baseUrl;
    private readonly ILogger<LocalAIProvider> _logger;

    public LocalAIProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<LocalAIProvider> logger)
    {
        _http = httpClientFactory.CreateClient("LocalAI");
        _baseUrl = config["AI:Local:BaseUrl"] ?? "http://localhost:11434";
        _model = config["AI:Local:Model"] ?? "llama3";
        _logger = logger;

        if (_http.BaseAddress == null)
            _http.BaseAddress = new Uri(_baseUrl.TrimEnd('/') + "/");
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

        messages.Add(new { role = "user", content = request.UserMessage });

        var body = new
        {
            model = _model,
            messages,
            stream = false
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.PostAsync("v1/chat/completions", content, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            return new AIChatResponse(text, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local AI endpoint ({BaseUrl}) unreachable or returned error.", _baseUrl);
            return new AIChatResponse($"[Local AI Notice: Unable to connect to local AI service at {_baseUrl}. Error: {ex.Message}]", true);
        }
    }

    public async Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input, CancellationToken ct = default)
    {
        var prompt = $"Local clinical review for {input.Modality ?? "medical"} image. " +
                     (input.ClinicalContext != null ? $"Context: {input.ClinicalContext}. " : "");

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Analyze image and summarize clinical observations."
        );

        var chatResponse = await ChatAsync(request, ct);

        return new MedicalImageAnalysisResult(
            Summary: chatResponse.Content,
            Findings: [],
            Observations: [],
            RegionsOfInterest: [],
            Confidence: null,
            RecommendationsForReview: ["Physician review required."]
        );
    }

    public async Task<LabAnalysisResult> AnalyzeLabResultsAsync(
        LabAnalysisInput input, CancellationToken ct = default)
    {
        var items = input.CurrentResult.Items ?? [];
        var labData = string.Join("\n",
            items.Select(i => $"- {i.TestName}: {i.Value} {i.Unit} ({i.AbnormalFlag ?? "Normal"})"));

        var prompt = $"Clinical lab evaluation:\n{labData}";
        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Evaluate these laboratory findings."
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
            Abnormalities: items.Where(i => !string.IsNullOrEmpty(i.AbnormalFlag))
                .Select(i => $"{i.TestName}: {i.Value} {i.Unit}")
                .ToList(),
            Trends: [],
            Recommendations: ["Physician review required."]
        );
    }

    public async Task<PatientSummaryResult> SummarizePatientAsync(
        PatientSummaryInput input, CancellationToken ct = default)
    {
        var prompt = $"Summarize patient {input.Patient.FirstName} {input.Patient.LastName}, Age {input.Patient.Age}.";
        var request = new AIChatRequest(SystemPrompt: prompt, History: [], UserMessage: "Summarize records.");
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
            SystemPrompt: $"Generate {input.ReportType} report.",
            History: [],
            UserMessage: input.Context
        );
        var response = await ChatAsync(request, ct);
        return new MedicalReportResult(response.Content, "markdown");
    }
}
