using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.DICOM.Commands;

public record ApproveDicomAICommand(Guid StudyId) : IRequest<Result<bool>>;

public class ApproveDicomAICommandHandler : IRequestHandler<ApproveDicomAICommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApproveDicomAICommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ApproveDicomAICommand request, CancellationToken cancellationToken)
    {
        var study = await _context.DicomStudies
            .FirstOrDefaultAsync(s => s.Id == request.StudyId
                && s.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (study is null) return Result<bool>.Failure("DICOM study not found.");
        if (!study.HasAIAnalysis) return Result<bool>.Failure("No AI analysis exists for this study.");

        study.AIReviewedByDoctor = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
