using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.FollowUp.Commands;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/follow-up")]
public class FollowUpIntelligenceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowUpIntelligenceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Generate AI follow-up suggestions. Doctor must approve before scheduling.</summary>
    [HttpPost("suggestions")]
    public async Task<IActionResult> Generate(
        Guid patientId,
        [FromQuery] Guid? visitId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GenerateFollowUpSuggestionsCommand(patientId, visitId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
