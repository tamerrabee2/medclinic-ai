using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.AITimeline.Queries;

public record GetPatientTimelineQuery(
    Guid PatientId,
    int Page = 1,
    int PageSize = 50,
    TimelineEventType? FilterType = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<Result<PaginatedList<TimelineEventDto>>>;

public record TimelineEventDto(
    Guid Id,
    TimelineEventType EventType,
    string Title,
    string? Description,
    DateTime EventDate,
    Guid? SourceEntityId,
    string? SourceEntityType,
    string? AISignificance,
    bool IsAIHighlighted,
    object? Metadata
);

public class GetPatientTimelineQueryHandler : IRequestHandler<GetPatientTimelineQuery, Result<PaginatedList<TimelineEventDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPatientTimelineQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<TimelineEventDto>>> Handle(GetPatientTimelineQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ClinicalTimelineEvents
            .Where(e => e.PatientId == request.PatientId && e.ClinicId == _currentUser.ClinicId)
            .AsQueryable();

        if (request.FilterType.HasValue)
            query = query.Where(e => e.EventType == request.FilterType.Value);

        if (request.From.HasValue)
            query = query.Where(e => e.EventDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.EventDate <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.EventDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new TimelineEventDto(
                e.Id,
                e.EventType,
                e.Title,
                e.Description,
                e.EventDate,
                e.SourceEntityId,
                e.SourceEntityType,
                e.AISignificance,
                e.IsAIHighlighted,
                null
            ))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<TimelineEventDto>>.Success(
            new PaginatedList<TimelineEventDto>(events, total, request.Page, request.PageSize));
    }
}
