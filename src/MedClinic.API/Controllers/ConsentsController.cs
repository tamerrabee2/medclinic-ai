using System.Security.Claims;
using MedClinic.API.Authorization;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Enums;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId:guid}/consents")]
public sealed class ConsentsController : ControllerBase
{
    private readonly IConsentService _service;

    public ConsentsController(IConsentService service)
    {
        _service = service;
    }

    [HttpGet]
    [HasPermission(Permissions.PatientConsentsView)]
    public async Task<ActionResult<IReadOnlyList<ConsentDto>>> History(Guid patientId, CancellationToken ct)
    {
        try
        {
            var history = await _service.GetHistoryAsync(patientId, ct);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("active/{type}")]
    [HasPermission(Permissions.PatientConsentsView)]
    public async Task<ActionResult<object>> HasActive(Guid patientId, ConsentType type, CancellationToken ct)
    {
        try
        {
            var hasActive = await _service.HasActiveConsentAsync(patientId, type, ct);
            return Ok(new
            {
                patientId,
                consentType = type,
                hasActiveConsent = hasActive
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [HasPermission(Permissions.PatientConsentsManage)]
    public async Task<ActionResult<ConsentDto>> Record(
        Guid patientId,
        [FromBody] RecordConsentRequest request,
        CancellationToken ct)
    {
        if (request.ExpiresAt is not null && request.ExpiresAt <= DateTime.UtcNow)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["expiresAt"] = new[] { "Expiry must be in the future." }
            }));
        }

        try
        {
            var currentUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : (Guid?)null;
            var result = await _service.RecordAsync(
                patientId,
                request.ConsentType,
                request.IsGranted,
                request.ExpiresAt,
                request.Notes,
                witnessUserId: currentUserId,
                grantedByUserId: currentUserId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                ct);

            return CreatedAtAction(nameof(History), new { patientId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{consentId:guid}/revoke")]
    [HasPermission(Permissions.PatientConsentsRevoke)]
    public async Task<ActionResult<ConsentDto>> Revoke(
        Guid patientId,
        Guid consentId,
        [FromBody] RevokeConsentRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["reason"] = new[] { "Revocation reason is required." }
            }));
        }

        try
        {
            var currentUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : (Guid?)null;
            var result = await _service.RevokeAsync(
                patientId,
                consentId,
                request.Reason,
                currentUserId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                ct);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("audit")]
    [HasPermission(Permissions.PatientConsentsAudit)]
    public async Task<ActionResult<IReadOnlyList<ConsentAuditEventDto>>> AuditTrail(
        Guid patientId,
        [FromQuery] Guid? consentId,
        CancellationToken ct)
    {
        try
        {
            var trail = await _service.GetAuditTrailAsync(patientId, consentId, ct);
            return Ok(trail);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public record RecordConsentRequest(
    ConsentType ConsentType,
    bool IsGranted,
    DateTime? ExpiresAt,
    string? Notes);

public record RevokeConsentRequest(string Reason);