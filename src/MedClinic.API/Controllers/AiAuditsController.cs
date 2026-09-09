using System.Security.Claims;
using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Features.AI.Services;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[ApiController]
[Route("api/v1/ai/audits")]
[Authorize]
public class AiAuditsController : BaseController
{
    private readonly IAiDecisionAuditService _auditService;

    public AiAuditsController(IAiDecisionAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// List and filter AI decision audits for the current clinic
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.ClinicAdmin},{Roles.SuperAdmin}")]
    public async Task<IActionResult> GetAudits(
        [FromQuery] AiAuditFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _auditService.GetAuditsAsync(filter, ct);
        return Success(result);
    }

    /// <summary>
    /// Get details of a specific AI decision audit by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.ClinicAdmin},{Roles.SuperAdmin}")]
    public async Task<IActionResult> GetAuditById(Guid id, CancellationToken ct)
    {
        var audit = await _auditService.GetAuditByIdAsync(id, ct);
        if (audit == null)
        {
            return NotFound("AI decision audit record not found.");
        }

        return Success(audit);
    }

    /// <summary>
    /// Review an AI decision (Approve, Reject, or Modify).
    /// Enforces human-in-the-loop clinical governance.
    /// OverrideReason is mandatory when Rejecting or Modifying.
    /// </summary>
    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = $"{Roles.Doctor},{Roles.ClinicAdmin},{Roles.SuperAdmin}")]
    public async Task<IActionResult> ReviewDecision(
        Guid id,
        [FromBody] ReviewAiDecisionRequest request,
        CancellationToken ct)
    {
        if (request.Status is Domain.Enums.AiReviewStatus.Rejected or Domain.Enums.AiReviewStatus.Modified)
        {
            if (string.IsNullOrWhiteSpace(request.OverrideReason))
            {
                return BadRequest("A clinical override reason is required when rejecting or modifying an AI clinical recommendation.");
            }
        }

        try
        {
            var updated = await _auditService.ReviewDecisionAsync(id, request, CurrentUserId, ct);
            return Success(new
            {
                auditId = updated.Id,
                reviewStatus = updated.ReviewStatus.ToString(),
                reviewedAt = updated.ReviewedAt,
                overrideReason = updated.OverrideReason
            }, "AI decision review status updated successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
