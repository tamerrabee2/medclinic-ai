using MediatR;
using MedClinic.Application.Features.Insurance.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/insurance")]
public class InsuranceController : ControllerBase
{
    private readonly IMediator _mediator;

    public InsuranceController(IMediator mediator) => _mediator = mediator;

    [HttpPost("claims")]
    public async Task<IActionResult> SubmitClaim([FromBody] SubmitInsuranceClaimRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitInsuranceClaimCommand(request.PatientId, request.InvoiceId, request.PolicyId, request.PayloadJson), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    [HttpPost("claims/{claimId}/status")]
    public async Task<IActionResult> CheckStatus(Guid claimId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckInsuranceClaimStatusCommand(claimId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }
}

public record SubmitInsuranceClaimRequest(Guid PatientId, Guid InvoiceId, Guid PolicyId, string PayloadJson);
