using MediatR;
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

    public ExternalLabsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitExternalLabRequest request, CancellationToken ct)
    {
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
