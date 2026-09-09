using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MedClinic.Application.Features.DICOM.Commands;

public record UploadDicomStudyCommand(
    Guid PatientId,
    Guid? VisitId,
    IFormFile DicomFile,
    string Modality,
    string? StudyDescription
) : IRequest<Result<DicomStudyUploadResultDto>>;

public record DicomStudyUploadResultDto(
    Guid StudyId,
    string StudyInstanceUid,
    string? ViewerUrl
);

public class UploadDicomStudyCommandHandler : IRequestHandler<UploadDicomStudyCommand, Result<DicomStudyUploadResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPacsProvider _pacs;

    public UploadDicomStudyCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPacsProvider pacs)
    {
        _context = context;
        _currentUser = currentUser;
        _pacs = pacs;
    }

    public async Task<Result<DicomStudyUploadResultDto>> Handle(
        UploadDicomStudyCommand request, CancellationToken cancellationToken)
    {
        var allowedExtensions = new[] { ".dcm", ".dicom" };
        var ext = Path.GetExtension(request.DicomFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext) && request.DicomFile.ContentType != "application/dicom")
            return Result<DicomStudyUploadResultDto>.Failure("Only DICOM files (.dcm) are accepted.");

        if (request.DicomFile.Length > 500 * 1024 * 1024) // 500 MB per file
            return Result<DicomStudyUploadResultDto>.Failure("DICOM file must be under 500 MB.");

        var pacsResult = await _pacs.UploadDicomFileAsync(
            request.DicomFile.OpenReadStream(),
            request.DicomFile.FileName,
            cancellationToken);

        if (!pacsResult.Success)
            return Result<DicomStudyUploadResultDto>.Failure($"PACS upload failed: {pacsResult.ErrorMessage}");

        var study = new DicomStudy
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            DoctorId = _currentUser.UserId,
            VisitId = request.VisitId,
            StudyInstanceUid = pacsResult.StudyInstanceUid!,
            StudyDescription = request.StudyDescription,
            StudyDate = DateTime.UtcNow,
            Modality = request.Modality.ToUpperInvariant(),
            WadoRsBaseUrl = _pacs.GetViewerUrl(pacsResult.StudyInstanceUid!),
            Status = DicomStudyStatus.Available
        };

        _context.DicomStudies.Add(study);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<DicomStudyUploadResultDto>.Success(new DicomStudyUploadResultDto(
            study.Id,
            study.StudyInstanceUid,
            _pacs.GetViewerUrl(study.StudyInstanceUid)
        ));
    }
}
