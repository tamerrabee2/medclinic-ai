using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.VoiceScribe.Commands;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/voice-scribe")]
public class VoiceScribeController : ControllerBase
{
    private readonly IMediator _mediator;

    public VoiceScribeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Upload a voice note for a patient.</summary>
    [HttpPost]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> Upload(
        [FromForm] Guid patientId,
        [FromForm] Guid? visitId,
        IFormFile audioFile,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateVoiceNoteCommand(patientId, visitId, audioFile), ct);
        return result.Succeeded ? Ok(new { id = result.Data }) : BadRequest(result.Errors);
    }

    /// <summary>Transcribe a voice note and extract structured clinical note (AI). Doctor must approve before use.</summary>
    [HttpPost("{id}/transcribe")]
    public async Task<IActionResult> Transcribe(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new TranscribeVoiceNoteCommand(id), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
    }

    /// <summary>Doctor approves (and optionally edits) AI-structured note.</summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveVoiceNoteRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveVoiceNoteCommand(
            id,
            req.ChiefComplaint,
            req.HistoryOfPresentIllness,
            req.PhysicalExamination,
            req.Assessment,
            req.Plan,
            req.AdditionalNotes), ct);
        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }
}

public record ApproveVoiceNoteRequest(
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? PhysicalExamination,
    string? Assessment,
    string? Plan,
    string? AdditionalNotes
);
