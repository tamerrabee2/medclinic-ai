using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.VoiceScribe.Commands;

public record TranscribeVoiceNoteCommand(Guid VoiceNoteId) : IRequest<Result<VoiceNoteTranscriptionResult>>;

public record VoiceNoteTranscriptionResult(
    string RawTranscript,
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? PhysicalExamination,
    string? Assessment,
    string? Plan,
    decimal Confidence
);

public class TranscribeVoiceNoteCommandHandler : IRequestHandler<TranscribeVoiceNoteCommand, Result<VoiceNoteTranscriptionResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIProvider _aiProvider;

    public TranscribeVoiceNoteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAIProvider aiProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _aiProvider = aiProvider;
    }

    public async Task<Result<VoiceNoteTranscriptionResult>> Handle(TranscribeVoiceNoteCommand request, CancellationToken cancellationToken)
    {
        var voiceNote = await _context.VoiceNotes
            .FirstOrDefaultAsync(v => v.Id == request.VoiceNoteId
                && v.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (voiceNote is null) return Result<VoiceNoteTranscriptionResult>.Failure("Voice note not found.");
        if (voiceNote.Status == VoiceNoteStatus.Completed) return Result<VoiceNoteTranscriptionResult>.Failure("Already transcribed.");

        voiceNote.Status = VoiceNoteStatus.Transcribing;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await _aiProvider.TranscribeVoiceNoteAsync(voiceNote.AudioFileUrl, cancellationToken);

            voiceNote.RawTranscript = result.RawTranscript;
            voiceNote.TranscriptLanguage = result.Language;
            voiceNote.TranscriptConfidence = result.Confidence;
            voiceNote.Status = VoiceNoteStatus.Processing;

            // Parse structured clinical note from transcript
            var structured = await _aiProvider.ParseClinicalNoteFromTranscriptAsync(result.RawTranscript, cancellationToken);

            voiceNote.ChiefComplaint = structured.ChiefComplaint;
            voiceNote.HistoryOfPresentIllness = structured.HistoryOfPresentIllness;
            voiceNote.PhysicalExamination = structured.PhysicalExamination;
            voiceNote.Assessment = structured.Assessment;
            voiceNote.Plan = structured.Plan;
            voiceNote.Status = VoiceNoteStatus.Completed;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<VoiceNoteTranscriptionResult>.Success(new VoiceNoteTranscriptionResult(
                result.RawTranscript,
                structured.ChiefComplaint,
                structured.HistoryOfPresentIllness,
                structured.PhysicalExamination,
                structured.Assessment,
                structured.Plan,
                result.Confidence
            ));
        }
        catch (Exception ex)
        {
            voiceNote.Status = VoiceNoteStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<VoiceNoteTranscriptionResult>.Failure($"Transcription failed: {ex.Message}");
        }
    }
}
