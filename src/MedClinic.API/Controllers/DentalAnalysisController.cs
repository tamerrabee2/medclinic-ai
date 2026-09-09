using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.Dental.Commands;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/dental-chart/analysis")]
public class DentalAnalysisController : ControllerBase
{
    private readonly IMediator _mediator;

    public DentalAnalysisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(Guid patientId, [FromQuery] Guid? visitId, CancellationToken ct)
    {
        var result = await _mediator.Send(new AnalyzeDentalChartCommand(patientId, visitId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
