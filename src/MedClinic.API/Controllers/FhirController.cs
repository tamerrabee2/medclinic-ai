using MediatR;
using MedClinic.Application.Features.FHIR.Commands;
using MedClinic.Application.Features.FHIR.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/fhir")]
public class FhirController : ControllerBase
{
    private readonly IMediator _mediator;

    public FhirController(IMediator mediator) => _mediator = mediator;

    [HttpPost("export")]
    public async Task<IActionResult> ExportPatient(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ExportPatientToFhirCommand(patientId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientFhirHistoryQuery(patientId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
