using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.Dental.Commands;
using MedClinic.Application.Features.Dental.Queries;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/dental-chart")]
public class DentalChartController : ControllerBase
{
    private readonly IMediator _mediator;

    public DentalChartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid patientId, [FromQuery] Guid? visitId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDentalChartQuery(patientId, visitId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(Guid patientId, [FromBody] UpsertDentalChartRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpsertDentalChartCommand(
            patientId,
            request.VisitId,
            request.Notes,
            request.Teeth.Select(t => new ToothRecordInput(t.ToothNumber, t.Status, t.Notes, t.SurfaceDataJson)).ToList()
        ), ct);

        return result.Succeeded ? Ok(new { id = result.Data }) : BadRequest(result.Errors);
    }
}

public record UpsertDentalChartRequest(Guid? VisitId, string Notes, List<ToothRecordRequest> Teeth);
public record ToothRecordRequest(int ToothNumber, MedClinic.Domain.Entities.ToothStatus Status, string? Notes, string SurfaceDataJson);
