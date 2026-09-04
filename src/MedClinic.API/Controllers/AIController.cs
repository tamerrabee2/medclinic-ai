using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Features.AI.Services;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    public AIController(AIService ai) => _ai = ai;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
        var result = await _ai.SendMessageAsync(CurrentUserId, req, ct);
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
        var result = await _ai.AnalyzeLabResultAsync(CurrentUserId, req, ct);
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
        var result = await _ai.GeneratePatientSummaryAsync(CurrentUserId, req, ct);
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
        var result = await _ai.AnalyzeImageAsync(CurrentUserId, req, ct);
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
