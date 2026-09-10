using MediatR;
using MedClinic.Application.Interfaces;
using MedClinic.Application.Features.ExternalLabs.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/external-labs")]
public class ExternalLabsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenant;
    private readonly ITenantEntitlementService _entitlement;

    public ExternalLabsController(
        IMediator mediator,
        ITenantContext tenant,
        ITenantEntitlementService entitlement)
    {
        _mediator = mediator;
        _tenant = tenant;
        _entitlement = entitlement;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitExternalLabRequest request, CancellationToken ct)
    {
        if (_tenant.ClinicId.HasValue)
        {
            var exec = await _entitlement.CheckCanExecuteAsync(_tenant.ClinicId.Value, Domain.Enums.ClinicalAction.OrderLab, ct);
            if (!exec.IsAllowed)
                return StatusCode(403, new { success = false, code = exec.Code, message = exec.Reason });
        }

        var result = await _mediator.Send(new SubmitExternalLabOrderCommand(request.PatientId, request.LabOrderId, request.ProviderId, request.PayloadJson), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("{syncId}/fetch")]
    public async Task<IActionResult> Fetch(Guid syncId, CancellationToken ct)
    {
        var result = await _mediator.Send(new FetchExternalLabResultCommand(syncId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}

public record SubmitExternalLabRequest(Guid PatientId, Guid LabOrderId, Guid ProviderId, string PayloadJson);
