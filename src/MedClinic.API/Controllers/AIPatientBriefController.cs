using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.AIPatientBrief.Commands;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/brief")]
public class AIPatientBriefController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIPatientBriefController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Generate an AI patient brief. Always requires doctor review before clinical use.</summary>
    [HttpPost]
    public async Task<IActionResult> Generate(Guid patientId, [FromQuery] Guid? visitId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GeneratePatientBriefCommand(patientId, visitId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
