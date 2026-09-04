using MedClinic.Application.Features.Canvas.DTOs;
using MedClinic.Application.Features.Canvas.Services;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedClinic.API.Controllers;

[ApiController]
[Route("api/v1/canvas")]
[Authorize]
public class CanvasController : ControllerBase
{
    private readonly CanvasService _canvas;
    public CanvasController(CanvasService canvas) => _canvas = canvas;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ───────────────────────────────────────────────────────────────────
    // Image Annotations
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Get all annotations for a medical image</summary>
    [HttpGet("images/{imageId}/annotations")]
    [Authorize(Policy = Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetAnnotations(
        Guid imageId, CancellationToken ct)
    {
        var result = await _canvas.GetAnnotationsAsync(imageId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Add a single annotation to a medical image</summary>
    [HttpPost("images/{imageId}/annotations")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> AddAnnotation(
        Guid imageId,
        [FromBody] CreateAnnotationRequest req,
        CancellationToken ct)
    {
        var result = await _canvas.AddAnnotationAsync(CurrentUserId, req with { MedicalImageId = imageId }, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete an annotation</summary>
    [HttpDelete("annotations/{annotationId}")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> DeleteAnnotation(
        Guid annotationId, CancellationToken ct)
    {
        await _canvas.DeleteAnnotationAsync(annotationId, CurrentUserId, ct);
        return Ok(new { success = true, message = "Annotation deleted." });
    }

    /// <summary>
    /// Save full canvas state: bulk annotations + annotated preview.
    /// The original image is NEVER modified.
    /// </summary>
    [HttpPost("save")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> SaveCanvas(
        [FromBody] SaveCanvasRequest req,
        CancellationToken ct)
    {
        await _canvas.SaveCanvasAsync(CurrentUserId, req, ct);
        return Ok(new { success = true, message = "Canvas saved successfully." });
    }

    // ───────────────────────────────────────────────────────────────────
    // Body Map
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Get body map annotations for a visit</summary>
    [HttpGet("body-map/{visitId}")]
    [Authorize(Policy = Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetBodyAnnotations(
        Guid visitId, CancellationToken ct)
    {
        var result = await _canvas.GetBodyAnnotationsAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Add a body map annotation pin for a visit</summary>
    [HttpPost("body-map")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> AddBodyAnnotation(
        [FromBody] CreateBodyAnnotationRequest req,
        CancellationToken ct)
    {
        var result = await _canvas.AddBodyAnnotationAsync(CurrentUserId, req, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a body map annotation</summary>
    [HttpDelete("body-map/annotations/{annotationId}")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> DeleteBodyAnnotation(
        Guid annotationId, CancellationToken ct)
    {
        await _canvas.DeleteBodyAnnotationAsync(annotationId, ct);
        return Ok(new { success = true, message = "Annotation deleted." });
    }

    // ───────────────────────────────────────────────────────────────────
    // Dental
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Get full dental chart for a patient</summary>
    [HttpGet("dental/{patientId}")]
    [Authorize(Policy = Permissions.PatientsRead)]
    public async Task<IActionResult> GetDentalChart(
        Guid patientId, CancellationToken ct)
    {
        var result = await _canvas.GetDentalChartAsync(patientId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create or update a tooth record (upsert by toothNumber + visitId)</summary>
    [HttpPut("dental")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> UpsertDentalRecord(
        [FromBody] UpsertDentalRecordRequest req,
        CancellationToken ct)
    {
        var result = await _canvas.UpsertDentalRecordAsync(CurrentUserId, req, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a dental record</summary>
    [HttpDelete("dental/{recordId}")]
    [Authorize(Policy = Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> DeleteDentalRecord(
        Guid recordId, CancellationToken ct)
    {
        await _canvas.DeleteDentalRecordAsync(recordId, ct);
        return Ok(new { success = true, message = "Dental record deleted." });
    }
}
