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

    private async Task<IActionResult> ExecuteWithQuotaReservationAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        CancellationToken ct,
        string? requestPayloadHash = null)
    {
        if (!_tenant.ClinicId.HasValue)
        {
            var directResult = await operation();
            return Ok(new { success = true, data = directResult });
        }

        var clinicId = _tenant.ClinicId.Value;
        var idempotencyKey = Request.Headers.TryGetValue("X-Idempotency-Key", out var headerVal) && !string.IsNullOrWhiteSpace(headerVal)
            ? headerVal.ToString()
            : Guid.NewGuid().ToString("N");

        var reservation = await _entitlement.ReserveQuotaAsync(
            clinicId: clinicId,
            metricType: MetricType.AiRequestsCount,
            delta: 1,
            idempotencyKey: idempotencyKey,
            operationId: $"{operationName}-{Guid.NewGuid():N}",
            ttl: TimeSpan.FromMinutes(5),
            requestPayloadHash: requestPayloadHash,
            ct: ct);

        if (!reservation.IsAllowed)
        {
            var statusCode = reservation.Code switch
            {
                "reservation_expired" or "operation_released" or "idempotency_key_reused" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status403Forbidden
            };
            return StatusCode(statusCode, new { success = false, code = reservation.Code, message = reservation.Reason });
        }

        try
        {
            var result = await operation();
            if (reservation.ReservationId.HasValue)
            {
                await _entitlement.CommitReservationAsync(reservation.ReservationId.Value, ct);
            }
            return Ok(new { success = true, data = result });
        }
        catch (MedClinic.Domain.Exceptions.QuotaReservationExpiredException ex)
        {
            return StatusCode(409, new { success = false, code = "reservation_expired", message = ex.Message });
        }
        catch (Exception ex)
        {
            if (reservation.ReservationId.HasValue)
            {
                await _entitlement.ReleaseReservationAsync(reservation.ReservationId.Value, ex.Message, ct);
            }
            throw;
        }
    }

    private static string ComputePayloadHash(params string?[] parts)
    {
        var raw = string.Join("|", parts.Where(p => !string.IsNullOrEmpty(p)));
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
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
    public Task<IActionResult> Chat(
        [FromBody] SendMessageRequest req,
        CancellationToken ct)
    {
        var payloadHash = ComputePayloadHash(req.Message, req.PatientContextId?.ToString());
        return ExecuteWithQuotaReservationAsync(
            "ai-chat",
            () => _ai.SendMessageAsync(CurrentUserId, req, ct),
            ct,
            payloadHash);
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
    public Task<IActionResult> AnalyzeLab(
        [FromBody] AnalyzeLabRequest req,
        CancellationToken ct)
    {
        return ExecuteWithQuotaReservationAsync(
            "ai-analyze-lab",
            () => _ai.AnalyzeLabResultAsync(CurrentUserId, req, ct),
            ct);
    }

    // ── Patient Summary ───────────────────────────────────────────────────────

    /// <summary>
    /// Generate an AI-powered patient summary.
    /// Includes medical history, active conditions, medications, and trends.
    /// ⚠️ For clinical decision support only — physician review required.
    /// </summary>
    [HttpPost("analyze/patient-summary")]
    [Authorize(Policy = Permissions.AIAnalysis)]
    public Task<IActionResult> GeneratePatientSummary(
        [FromBody] GeneratePatientSummaryRequest req,
        CancellationToken ct)
    {
        return ExecuteWithQuotaReservationAsync(
            "ai-patient-summary",
            () => _ai.GeneratePatientSummaryAsync(CurrentUserId, req, ct),
            ct);
    }

    // ── Medical Image Analysis ────────────────────────────────────────────────

    /// <summary>
    /// Analyze a medical image using AI (X-Ray, CT, MRI, Ultrasound).
    /// Provides findings and observations.
    /// ⚠️ AI does NOT replace radiologist review — physician approval required.
    /// </summary>
    [HttpPost("analyze/image")]
    [Authorize(Policy = Permissions.AIAnalysis)]
    public Task<IActionResult> AnalyzeImage(
        [FromBody] AnalyzeImageRequest req,
        CancellationToken ct)
    {
        return ExecuteWithQuotaReservationAsync(
            "ai-analyze-image",
            () => _ai.AnalyzeImageAsync(CurrentUserId, req, ct),
            ct);
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
