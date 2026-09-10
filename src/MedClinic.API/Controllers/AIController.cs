using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Features.AI.Services;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using MedClinic.Application.Interfaces;
using MedClinic.Domain.Enums;

namespace MedClinic.API.Controllers;

/// <summary>
/// AI Medical Assistant — Chat, Lab Analysis, Patient Summary, Image Analysis.
/// All AI outputs require physician review before clinical action.
/// ⚠️ AI-generated content is for clinical decision support only.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Authorize]
public class AIController : ControllerBase
{
    private readonly AIService _ai;
    private readonly ITenantEntitlementService _entitlement;
    private readonly ITenantContext _tenant;

    public AIController(
        AIService ai,
        ITenantEntitlementService entitlement,
        ITenantContext tenant)
    {
        _ai = ai;
        _entitlement = entitlement;
        _tenant = tenant;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<IActionResult?> GuardAiEntitlementAsync(CancellationToken ct)
    {
        if (!_tenant.ClinicId.HasValue) return null;
        var clinicId = _tenant.ClinicId.Value;

        var exec = await _entitlement.CheckCanExecuteAsync(clinicId, ClinicalAction.UseAiCopilot, ct);
        if (!exec.IsAllowed)
            return StatusCode(403, new { success = false, code = exec.Code, message = exec.Reason });

        var hasFeature = await _entitlement.HasFeatureAsync(clinicId, FeatureKey.AiCopilot, ct);
        if (!hasFeature)
            return StatusCode(403, new { success = false, code = "feature_not_included", message = "AI Copilot is not included in your clinic's subscription plan." });

        var quota = await _entitlement.CheckQuotaAsync(clinicId, MetricType.AiRequestsCount, ct);
        if (!quota.IsAllowed)
            return StatusCode(403, new { success = false, code = quota.Code, message = quota.Reason });

        return null;
    }

    private async Task RecordAiUsageAsync(CancellationToken ct)
    {
        if (_tenant.ClinicId.HasValue)
        {
            await _entitlement.RecordUsageAsync(_tenant.ClinicId.Value, MetricType.AiRequestsCount, 1, ct);
        }
    }

    // ── Conversations ────────────────────────────────────────────────────────

    /// <summary>List all AI conversations for the current user</summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var result = await _ai.GetConversationsAsync(CurrentUserId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get a specific conversation with full message history</summary>
    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(Guid id, CancellationToken ct)
    {
        var result = await _ai.GetConversationAsync(id, CurrentUserId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Send a message to the AI assistant.
    /// Optionally attach a patientContextId to inject patient data into the AI context.
    /// Optionally send an image attachment as base64 for vision-capable models.
    /// </summary>
    [HttpPost("chat")]
    [Authorize(Policy = Permissions.AIAnalysis)]
    public async Task<IActionResult> Chat(
        [FromBody] SendMessageRequest req,
        CancellationToken ct)
    {
        var guardResult = await GuardAiEntitlementAsync(ct);
        if (guardResult is not null) return guardResult;

        var result = await _ai.SendMessageAsync(CurrentUserId, req, ct);
        await RecordAiUsageAsync(ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a conversation and all its messages</summary>
    [HttpDelete("conversations/{id}")]
    public async Task<IActionResult> DeleteConversation(Guid id, CancellationToken ct)
    {
        await _ai.DeleteConversationAsync(id, CurrentUserId, ct);
        return Ok(new { success = true, message = "Conversation deleted." });
    }

    // ── Lab Analysis ─────────────────────────────────────────────────────────

    /// <summary>
    /// Analyze lab results using AI.
    /// Compares with previous results for trend analysis.
    /// ⚠️ Requires physician review — AI does not make final clinical decisions.
    /// </summary>
    [HttpPost("analyze/lab")]
    [Authorize(Policy = Permissions.AIAnalysis)]
    public async Task<IActionResult> AnalyzeLab(
        [FromBody] AnalyzeLabRequest req,
        CancellationToken ct)
    {
        var guardResult = await GuardAiEntitlementAsync(ct);
        if (guardResult is not null) return guardResult;

        var result = await _ai.AnalyzeLabResultAsync(CurrentUserId, req, ct);
        await RecordAiUsageAsync(ct);
        return Ok(new { success = true, data = result });
    }

    // ── Patient Summary ───────────────────────────────────────────────────────

    /// <summary>
    /// Generate an AI-powered patient summary.
    /// Includes medical history, active conditions, medications, and trends.
    /// ⚠️ For clinical decision support only — physician review required.
    /// </summary>
    [HttpPost("analyze/patient-summary")]
    [Authorize(Policy = Permissions.AIAnalysis)]
    public async Task<IActionResult> GeneratePatientSummary(
        [FromBody] GeneratePatientSummaryRequest req,
        CancellationToken ct)
    {
        var guardResult = await GuardAiEntitlementAsync(ct);
        if (guardResult is not null) return guardResult;

        var result = await _ai.GeneratePatientSummaryAsync(CurrentUserId, req, ct);
        await RecordAiUsageAsync(ct);
        return Ok(new { success = true, data = result });
    }

    // ── Medical Image Analysis ────────────────────────────────────────────────

    /// <summary>
    /// Analyze a medical image using AI (X-Ray, CT, MRI, Ultrasound).
    /// Provides findings and observations.
    /// ⚠️ AI does NOT replace radiologist review — physician approval required.
    /// </summary>
    [HttpPost("analyze/image")]
    [Authorize(Policy = Permissions.AIAnalysis)]
    public async Task<IActionResult> AnalyzeImage(
        [FromBody] AnalyzeImageRequest req,
        CancellationToken ct)
    {
        var guardResult = await GuardAiEntitlementAsync(ct);
        if (guardResult is not null) return guardResult;

        var result = await _ai.AnalyzeImageAsync(CurrentUserId, req, ct);
        await RecordAiUsageAsync(ct);
        return Ok(new { success = true, data = result });
    }

    // ── Meta ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get available AI features and current provider info.
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            success = true,
            data = new
            {
                features = new[]
                {
                    "AI Medical Chat Assistant (Dr. AI)",
                    "Lab Result Analysis with Trend Detection",
                    "Patient Summary Generation",
                    "Medical Image Analysis (X-Ray, CT, MRI, Ultrasound)"
                },
                disclaimer =
                    "⚠️ All AI-generated content is for clinical decision support only " +
                    "and must be reviewed by a qualified healthcare professional " +
                    "before any clinical action is taken.",
                requiresDoctorReview = true
            }
        });
    }
}
