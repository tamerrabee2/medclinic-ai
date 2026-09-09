using MediatR;
using MedClinic.Application.Features.PatientPortal.Commands;
using MedClinic.Application.Features.PatientPortal.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/portal")]
public class PatientPortalController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientPortalController(IMediator mediator) => _mediator = mediator;

    [HttpPost("provision")]
    public async Task<IActionResult> Provision(Guid patientId, [FromBody] ProvisionPortalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ProvisionPatientPortalAccessCommand(patientId, request.Email, request.PasswordHash), ct);
        return result.Succeeded ? Ok(new { id = result.Data }) : BadRequest(result.Errors);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage(Guid patientId, [FromBody] SendPortalMessageRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendPatientPortalMessageCommand(patientId, request.SenderType, request.Subject, request.Body), ct);
        return result.Succeeded ? Ok(new { id = result.Data }) : BadRequest(result.Errors);
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientPortalMessagesQuery(patientId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}

public record ProvisionPortalRequest(string Email, string PasswordHash);
public record SendPortalMessageRequest(string SenderType, string Subject, string Body);
