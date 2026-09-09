using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MedClinic.Infrastructure.AI;

/// <summary>
/// Google Gemini AI provider — requires AI:Gemini:ApiKey in configuration.
/// Supports Gemini 1.5 Pro, Flash, and 2.0.
/// </summary>
public class GeminiProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiProvider> _logger;

    public GeminiProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<GeminiProvider> logger)
    {
        _http = httpClientFactory.CreateClient("Gemini");
        _apiKey = config["AI:Gemini:ApiKey"] ?? string.Empty;
        _model = config["AI:Gemini:Model"] ?? "gemini-1.5-flash";
        _logger = logger;
    }

    public async Task<AIChatResponse> ChatAsync(
        AIChatRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Falling back to mock message.");
            return new AIChatResponse("Gemini API key is not configured. Please set AI:Gemini:ApiKey.", false);
        }

        var contents = new List<object>();

        // History
        foreach (var h in request.History)
        {
            var geminiRole = h.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "model" : "user";
            contents.Add(new
            {
                role = geminiRole,
                parts = new object[] { new { text = h.Content } }
            });
        }

        // Current user message + optional attachment
        var currentParts = new List<object>
        {
            new { text = request.UserMessage }
        };

        if (!string.IsNullOrEmpty(request.AttachmentBase64))
        {
            currentParts.Add(new
            {
                inline_data = new
                {
                    mime_type = request.AttachmentMimeType ?? "image/jpeg",
                    data = request.AttachmentBase64
                }
            });
        }

        contents.Add(new
        {
            role = "user",
            parts = currentParts
        });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new object[] { new { text = request.SystemPrompt } }
            },
            contents
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);

        var candidate = doc.RootElement.GetProperty("candidates")[0];
        var text = candidate
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        return new AIChatResponse(text, false);
    }

    public async Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input, CancellationToken ct = default)
    {
        var prompt =
            $"You are an expert radiology AI assistant. Analyze this {input.Modality ?? "medical"} image. " +
            (input.ClinicalContext != null
                ? $"Clinical context: {input.ClinicalContext}. "
                : "") +
            "Provide structured findings: summary, key observations, any regions of interest, and clinical recommendations. " +
            "Mandatory disclaimer: Physician review is strictly required.";

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Please analyze this medical image with clinical scrutiny."
        );

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
                $"- {i.TestName}: {i.Value} {i.Unit} (Ref: {i.ReferenceRange}, Flag: {i.AbnormalFlag ?? "Normal"})"));

        var prompt = "You are a clinical laboratory AI assistant. " +
            "Analyze the following lab results and provide:\n" +
            "1. Clinical summary\n" +
            "2. Critical/Abnormal findings and potential etiology\n" +
            "3. Trend analysis\n" +
            "4. Recommended medical follow-ups\n\n" +
            $"Lab Results:\n{labData}";

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Analyze these lab results."
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
        var visits = string.Join("\n", input.RecentVisits
            .Select(v => $"- {v.VisitDate:yyyy-MM-dd}: {v.ChiefComplaint}"));

        var prompt =
            $"Summarize medical record for patient {input.Patient.FirstName} {input.Patient.LastName}, " +
            $"Age: {input.Patient.Age}, Gender: {input.Patient.Gender}.\n\n" +
            $"Recent visits:\n{(string.IsNullOrEmpty(visits) ? "None recorded" : visits)}\n\n" +
            "Provide concise highlights of conditions, medications, abnormal findings, and care plan.";

        var request = new AIChatRequest(
            SystemPrompt: prompt,
            History: [],
            UserMessage: "Generate patient clinical summary."
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
            SystemPrompt: $"Generate a comprehensive {input.ReportType} medical report based on provided context.",
            History: [],
            UserMessage: input.Context
        );
        var response = await ChatAsync(request, ct);
        return new MedicalReportResult(response.Content, "markdown");
    }
}
