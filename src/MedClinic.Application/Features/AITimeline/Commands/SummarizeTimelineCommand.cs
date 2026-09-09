using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.AITimeline.Commands;

public record SummarizeTimelineCommand(Guid PatientId) : IRequest<Result<TimelineSummaryDto>>;

public record TimelineSummaryDto(
    string NarrativeSummary,
    List<string> KeyChanges,
    List<string> CriticalEvents,
    string TrendAssessment,
    DateTime GeneratedAt
);

public class SummarizeTimelineCommandHandler : IRequestHandler<SummarizeTimelineCommand, Result<TimelineSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIProvider _aiProvider;

    public SummarizeTimelineCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAIProvider aiProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _aiProvider = aiProvider;
    }

    public async Task<Result<TimelineSummaryDto>> Handle(SummarizeTimelineCommand request, CancellationToken cancellationToken)
    {
        var events = await _context.ClinicalTimelineEvents
            .Where(e => e.PatientId == request.PatientId && e.ClinicId == _currentUser.ClinicId)
            .OrderByDescending(e => e.EventDate)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (!events.Any())
            return Result<TimelineSummaryDto>.Failure("No timeline events found for this patient.");

        var result = await _aiProvider.SummarizePatientTimelineAsync(events, cancellationToken);

        // Mark AI-highlighted events
        if (result.HighlightedEventIds?.Any() == true)
        {
            var toHighlight = events.Where(e => result.HighlightedEventIds.Contains(e.Id)).ToList();
            foreach (var e in toHighlight) e.IsAIHighlighted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<TimelineSummaryDto>.Success(new TimelineSummaryDto(
            result.NarrativeSummary,
            result.KeyChanges,
            result.CriticalEvents,
            result.TrendAssessment,
            DateTime.UtcNow
        ));
    }
}
