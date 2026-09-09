using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MedClinic.Application.Features.VoiceScribe.Commands;

public record CreateVoiceNoteCommand(
    Guid PatientId,
    Guid? VisitId,
    IFormFile AudioFile
) : IRequest<Result<Guid>>;

public class CreateVoiceNoteCommandHandler : IRequestHandler<CreateVoiceNoteCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IAIProvider _aiProvider;

    public CreateVoiceNoteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage,
        IAIProvider aiProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _aiProvider = aiProvider;
    }

    public async Task<Result<Guid>> Handle(CreateVoiceNoteCommand request, CancellationToken cancellationToken)
    {
        var allowedTypes = new[] { "audio/webm", "audio/mp4", "audio/mpeg", "audio/ogg", "audio/wav" };
        if (!allowedTypes.Contains(request.AudioFile.ContentType))
            return Result<Guid>.Failure("Invalid audio file type.");

        if (request.AudioFile.Length > 50 * 1024 * 1024)
            return Result<Guid>.Failure("Audio file must be under 50 MB.");

        var fileUrl = await _fileStorage.UploadAsync(
            request.AudioFile.OpenReadStream(),
            request.AudioFile.FileName,
            request.AudioFile.ContentType,
            "voice-notes",
            cancellationToken);

        var voiceNote = new VoiceNote
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            DoctorId = _currentUser.UserId!.Value,
            VisitId = request.VisitId,
            AudioFileUrl = fileUrl,
            AudioFileSizeBytes = request.AudioFile.Length,
            MimeType = request.AudioFile.ContentType,
            Status = VoiceNoteStatus.Pending
        };

        _context.VoiceNotes.Add(voiceNote);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(voiceNote.Id);
    }
}
