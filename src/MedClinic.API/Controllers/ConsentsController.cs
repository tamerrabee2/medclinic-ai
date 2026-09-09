using System.Security.Claims;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MedClinic.API.Controllers;
[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId:guid}/consents")]
public sealed class ConsentsController : ControllerBase
{
    private readonly IConsentService _service;
    public ConsentsController(IConsentService service) => _service = service;
    [HttpGet] public async Task<ActionResult<IReadOnlyList<ConsentDto>>> History(Guid patientId, CancellationToken ct) => Ok(await _service.GetHistoryAsync(patientId, ct));
    [HttpGet("active/{type}")] public async Task<ActionResult<object>> HasActive(Guid patientId, ConsentType type, CancellationToken ct) => Ok(new { patientId, consentType = type, hasActiveConsent = await _service.HasActiveConsentAsync(patientId, type, ct) });
    [HttpPost] public async Task<ActionResult<ConsentDto>> Record(Guid patientId, [FromBody] RecordConsentRequest request, CancellationToken ct)
    {
        if (request.ExpiresAt is not null && request.ExpiresAt <= DateTime.UtcNow) return ValidationProblem(new Dictionary<string,string[]> { ["expiresAt"] = new[]{ "Expiry must be in the future." } });
        var witness = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : (Guid?)null;
        var result = await _service.RecordAsync(patientId, request.ConsentType, request.IsGranted, request.ExpiresAt, request.Notes, witness, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return CreatedAtAction(nameof(History), new { patientId }, result);
    }
    [HttpPost("{consentId:guid}/revoke")] public async Task<ActionResult<ConsentDto>> Revoke(Guid patientId, Guid consentId, [FromBody] RevokeConsentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return ValidationProblem(new Dictionary<string,string[]> { ["reason"] = new[]{ "Revocation reason is required." } });
        return Ok(await _service.RevokeAsync(patientId, consentId, request.Reason, ct));
    }
}
public record RecordConsentRequest(ConsentType ConsentType, bool IsGranted, DateTime? ExpiresAt, string? Notes);
public record RevokeConsentRequest(string Reason);