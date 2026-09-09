using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.DICOM.Commands;

public record AnalyzeDicomStudyCommand(Guid StudyId) : IRequest<Result<DicomAIAnalysisDto>>;

public record DicomAIAnalysisDto(
    string Findings,
    List<string> Impressions,
    List<string> Recommendations,
    decimal ConfidenceScore,
    bool RequiresDoctorReview
);

public class AnalyzeDicomStudyCommandHandler
    : IRequestHandler<AnalyzeDicomStudyCommand, Result<DicomAIAnalysisDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIProvider _aiProvider;

    public AnalyzeDicomStudyCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAIProvider aiProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _aiProvider = aiProvider;
    }

    public async Task<Result<DicomAIAnalysisDto>> Handle(
        AnalyzeDicomStudyCommand request, CancellationToken cancellationToken)
    {
        var study = await _context.DicomStudies
            .FirstOrDefaultAsync(s => s.Id == request.StudyId
                && s.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (study is null)
            return Result<DicomAIAnalysisDto>.Failure("DICOM study not found.");

        if (study.Status != Domain.Entities.DicomStudyStatus.Available)
            return Result<DicomAIAnalysisDto>.Failure("Study is not yet available for analysis.");

        var result = await _aiProvider.AnalyzeDicomStudyAsync(
            study.StudyInstanceUid, study.Modality, cancellationToken);

        study.HasAIAnalysis = true;
        study.AIFindings = result.Findings;
        study.AIReviewedByDoctor = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<DicomAIAnalysisDto>.Success(new DicomAIAnalysisDto(
            result.Findings,
            result.Impressions,
            result.Recommendations,
            result.ConfidenceScore,
            RequiresDoctorReview: true
        ));
    }
}
