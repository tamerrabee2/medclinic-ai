using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.DICOM.Queries;

public record GetPatientDicomStudiesQuery(
    Guid PatientId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<DicomStudySummaryDto>>>;

public record DicomStudySummaryDto(
    Guid Id,
    string StudyInstanceUid,
    string? AccessionNumber,
    string? StudyDescription,
    DateTime StudyDate,
    string Modality,
    int SeriesCount,
    int InstanceCount,
    DicomStudyStatus Status,
    string? ViewerUrl,
    bool HasAIAnalysis,
    bool AIReviewedByDoctor
);

public class GetPatientDicomStudiesQueryHandler
    : IRequestHandler<GetPatientDicomStudiesQuery, Result<PaginatedList<DicomStudySummaryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPacsProvider _pacs;

    public GetPatientDicomStudiesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPacsProvider pacs)
    {
        _context = context;
        _currentUser = currentUser;
        _pacs = pacs;
    }

    public async Task<Result<PaginatedList<DicomStudySummaryDto>>> Handle(
        GetPatientDicomStudiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DicomStudies
            .Where(s => s.PatientId == request.PatientId && s.ClinicId == _currentUser.ClinicId);

        var total = await query.CountAsync(cancellationToken);

        var studies = await query
            .OrderByDescending(s => s.StudyDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new DicomStudySummaryDto(
                s.Id,
                s.StudyInstanceUid,
                s.AccessionNumber,
                s.StudyDescription,
                s.StudyDate,
                s.Modality,
                s.SeriesCount,
                s.InstanceCount,
                s.Status,
                null, // viewer URL resolved at API layer
                s.HasAIAnalysis,
                s.AIReviewedByDoctor
            ))
            .ToListAsync(cancellationToken);

        // Resolve viewer URLs
        var enriched = studies
            .Select(s => s with { ViewerUrl = _pacs.GetViewerUrl(s.StudyInstanceUid) })
            .ToList();

        return Result<PaginatedList<DicomStudySummaryDto>>.Success(
            new PaginatedList<DicomStudySummaryDto>(enriched, total, request.Page, request.PageSize));
    }
}
