using MediatR;
using MedClinic.Application.Interfaces;
using MedClinic.Application.Features.DICOM.Commands;
using MedClinic.Application.Features.DICOM.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId}/dicom")]
public class DicomStudyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenant;
    private readonly ITenantEntitlementService _entitlement;

    public DicomStudyController(
        IMediator mediator,
        ITenantContext tenant,
        ITenantEntitlementService entitlement)
    {
        _mediator = mediator;
        _tenant = tenant;
        _entitlement = entitlement;
    }

    /// <summary>List all DICOM studies for a patient.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPatientDicomStudiesQuery(patientId, page, pageSize), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    /// <summary>Upload a DICOM file (.dcm) to PACS.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(524_288_000)] // 500 MB
    public async Task<IActionResult> Upload(
        Guid patientId,
        [FromForm] Guid? visitId,
        [FromForm] string modality,
        [FromForm] string? studyDescription,
        IFormFile dicomFile,
        CancellationToken ct)
    {
        if (_tenant.ClinicId.HasValue)
        {
            var exec = await _entitlement.CheckCanExecuteAsync(_tenant.ClinicId.Value, Domain.Enums.ClinicalAction.UploadDicom, ct);
            if (!exec.IsAllowed)
                return StatusCode(403, new { success = false, code = exec.Code, message = exec.Reason });
        }

        var result = await _mediator.Send(new UploadDicomStudyCommand(
            patientId, visitId, dicomFile, modality, studyDescription), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    /// <summary>Run AI analysis on a DICOM study. Doctor review required before clinical use.</summary>
    [HttpPost("{studyId}/analyze")]
    public async Task<IActionResult> Analyze(Guid studyId, CancellationToken ct)
    {
        if (_tenant.ClinicId.HasValue)
        {
            var exec = await _entitlement.CheckCanExecuteAsync(_tenant.ClinicId.Value, Domain.Enums.ClinicalAction.UseAiCopilot, ct);
            if (!exec.IsAllowed)
                return StatusCode(403, new { success = false, code = exec.Code, message = exec.Reason });

            var idempotencyKey = Request.Headers.TryGetValue("X-Idempotency-Key", out var headerVal) && !string.IsNullOrWhiteSpace(headerVal)
                ? headerVal.ToString()
                : Guid.NewGuid().ToString("N");

            var payloadHash = ComputePayloadHash(new { studyId });

            var reservation = await _entitlement.ReserveQuotaAsync(
                clinicId: _tenant.ClinicId.Value,
                metricType: Domain.Enums.MetricType.AiRequestsCount,
                delta: 1,
                idempotencyKey: idempotencyKey,
                operationId: $"dicom-ai-{Guid.NewGuid():N}",
                ttl: TimeSpan.FromMinutes(5),
                requestPayloadHash: payloadHash,
                ct: ct);

            if (!reservation.IsAllowed)
            {
                var statusCode = reservation.Code switch
                {
                    "reservation_expired" or "operation_released" or "idempotency_key_reused" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status403Forbidden
                };
                return StatusCode(statusCode, new { success = false, code = reservation.Code, message = reservation.Reason });
            }

            try
            {
                var result = await _mediator.Send(new AnalyzeDicomStudyCommand(studyId), ct);
                if (reservation.ReservationId.HasValue)
                {
                    await _entitlement.CommitReservationAsync(reservation.ReservationId.Value, ct);
                }
                return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                if (reservation.ReservationId.HasValue)
                {
                    await _entitlement.ReleaseReservationAsync(reservation.ReservationId.Value, ex.Message, ct);
                }
                throw;
            }
        }

        var directResult = await _mediator.Send(new AnalyzeDicomStudyCommand(studyId), ct);
        return directResult.Succeeded ? Ok(directResult.Data) : BadRequest(directResult.Errors);
    }

    private static string ComputePayloadHash(object req)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(req);
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }

    /// <summary>Doctor approves AI findings for a DICOM study.</summary>
    [HttpPost("{studyId}/approve-ai")]
    public async Task<IActionResult> ApproveAI(Guid studyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveDicomAICommand(studyId), ct);
        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }
}
