using MedClinic.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId:guid}/clinical-ai")]
public sealed class AdvancedClinicalAIController : ControllerBase
{
    private readonly IAdvancedClinicalAIService _service;
    public AdvancedClinicalAIController(IAdvancedClinicalAIService service) => _service = service;
    [HttpPost("risk-score")]
    public async Task<ActionResult<RiskScoreResult>> RiskScore(Guid patientId, [FromBody] RiskScoreApiRequest request, CancellationToken ct) => Ok(await _service.CalculateRiskScoreAsync(new RiskScoreRequest(patientId, request.Age, request.Vitals, request.Factors), ct));
    [HttpPost("soap-draft")]
    public async Task<ActionResult<SoapNoteDraft>> SoapDraft(Guid patientId, [FromBody] SoapNoteApiRequest request, CancellationToken ct) { if (string.IsNullOrWhiteSpace(request.FreeTextNote)) return ValidationProblem(new Dictionary<string, string[]> { ["freeTextNote"] = new[] { "A free-text note is required." } }); return Ok(await _service.SummarizeSoapNoteAsync(new SoapNoteRequest(patientId, request.FreeTextNote, request.VisitContext), ct)); }
    [HttpPost("triage")]
    public async Task<ActionResult<TriageAssessment>> Triage(Guid patientId, [FromBody] TriageApiRequest request, CancellationToken ct) { if (request.Symptoms is null || request.Symptoms.Count == 0) return ValidationProblem(new Dictionary<string, string[]> { ["symptoms"] = new[] { "At least one symptom is required." } }); return Ok(await _service.AssessTriageAsync(new TriageRequest(patientId, request.Symptoms, request.Vitals, request.Notes), ct)); }
    [HttpPost("radiology-report-draft")]
    public async Task<ActionResult<RadiologyReportDraft>> RadiologyDraft(Guid patientId, [FromBody] RadiologyReportApiRequest request, CancellationToken ct) { if (string.IsNullOrWhiteSpace(request.Modality) || string.IsNullOrWhiteSpace(request.Findings)) return ValidationProblem(new Dictionary<string, string[]> { ["request"] = new[] { "Modality and findings are required." } }); return Ok(await _service.GenerateRadiologyReportDraftAsync(new RadiologyReportRequest(patientId, request.Modality, request.BodyPart, request.Findings, request.Impressions), ct)); }
}
public sealed record RiskScoreApiRequest(int? Age, Dictionary<string, string>? Vitals, Dictionary<string, string>? Factors);
public sealed record SoapNoteApiRequest(string FreeTextNote, string? VisitContext);
public sealed record TriageApiRequest(List<string> Symptoms, Dictionary<string, string>? Vitals, string? Notes);
public sealed record RadiologyReportApiRequest(string Modality, string? BodyPart, string Findings, List<string>? Impressions);
