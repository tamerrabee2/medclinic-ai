using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.AITimeline.Queries;
using MedClinic.Application.Features.AITimeline.Commands;
using MedClinic.Domain.Entities;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/timeline")]
public class ClinicalTimelineController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicalTimelineController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get patient clinical timeline with pagination and filtering.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] TimelineEventType? type = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetPatientTimelineQuery(patientId, page, pageSize, type, from, to), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    /// <summary>AI summary of patient timeline. Doctor review required before clinical use.</summary>
    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize(Guid patientId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SummarizeTimelineCommand(patientId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
