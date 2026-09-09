using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.Reports.Queries;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get aggregated report data for a date range.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string type = "General",
        CancellationToken ct = default)
    {
        if (from > to) return BadRequest("'from' must be before 'to'.");
        var result = await _mediator.Send(new GetReportDataQuery(from, to, type), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
