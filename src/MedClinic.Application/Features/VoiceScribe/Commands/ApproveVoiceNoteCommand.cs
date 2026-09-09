using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.VoiceScribe.Commands;

public record ApproveVoiceNoteCommand(
    Guid VoiceNoteId,
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? PhysicalExamination,
    string? Assessment,
    string? Plan,
    string? AdditionalNotes
) : IRequest<Result<bool>>;

public class ApproveVoiceNoteCommandHandler : IRequestHandler<ApproveVoiceNoteCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApproveVoiceNoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ApproveVoiceNoteCommand request, CancellationToken cancellationToken)
    {
        var voiceNote = await _context.VoiceNotes
            .FirstOrDefaultAsync(v => v.Id == request.VoiceNoteId
                && v.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (voiceNote is null) return Result<bool>.Failure("Voice note not found.");

        voiceNote.ChiefComplaint = request.ChiefComplaint ?? voiceNote.ChiefComplaint;
        voiceNote.HistoryOfPresentIllness = request.HistoryOfPresentIllness ?? voiceNote.HistoryOfPresentIllness;
        voiceNote.PhysicalExamination = request.PhysicalExamination ?? voiceNote.PhysicalExamination;
        voiceNote.Assessment = request.Assessment ?? voiceNote.Assessment;
        voiceNote.Plan = request.Plan ?? voiceNote.Plan;
        voiceNote.AdditionalNotes = request.AdditionalNotes;
        voiceNote.DoctorApproved = true;
        voiceNote.DoctorApprovedAt = DateTime.UtcNow;
        voiceNote.ApprovedByDoctorId = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
