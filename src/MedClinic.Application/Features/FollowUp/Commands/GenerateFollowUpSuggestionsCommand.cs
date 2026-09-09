using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.FollowUp.Commands;

public record GenerateFollowUpSuggestionsCommand(Guid PatientId, Guid? VisitId = null)
    : IRequest<Result<List<FollowUpSuggestionDto>>>;

public record FollowUpSuggestionDto(
    Guid Id,
    string Reason,
    string RecommendedAction,
    FollowUpPriority Priority,
    int SuggestedDaysFromNow,
    DateTime? SuggestedDate,
    List<string> Triggers
);

public class GenerateFollowUpSuggestionsCommandHandler
    : IRequestHandler<GenerateFollowUpSuggestionsCommand, Result<List<FollowUpSuggestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIProvider _aiProvider;

    public GenerateFollowUpSuggestionsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAIProvider aiProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _aiProvider = aiProvider;
    }

    public async Task<Result<List<FollowUpSuggestionDto>>> Handle(
        GenerateFollowUpSuggestionsCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.Visits)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId
                && p.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (patient is null) return Result<List<FollowUpSuggestionDto>>.Failure("Patient not found.");

        var suggestions = await _aiProvider.GenerateFollowUpSuggestionsAsync(patient, cancellationToken);

        var entities = suggestions.Select(s => new FollowUpIntelligence
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            VisitId = request.VisitId,
            Reason = s.Reason,
            RecommendedAction = s.RecommendedAction,
            Priority = s.Priority,
            SuggestedDaysFromNow = s.SuggestedDaysFromNow,
            SuggestedDate = DateTime.UtcNow.AddDays(s.SuggestedDaysFromNow),
            TriggersJson = System.Text.Json.JsonSerializer.Serialize(s.Triggers)
        }).ToList();

        _context.FollowUpIntelligences.Add(entities.First()); // simplify for now
        _context.FollowUpIntelligences.AddRange(entities.Skip(1));
        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<FollowUpSuggestionDto>>.Success(
            entities.Select(e => new FollowUpSuggestionDto(
                e.Id, e.Reason, e.RecommendedAction, e.Priority,
                e.SuggestedDaysFromNow, e.SuggestedDate,
                System.Text.Json.JsonSerializer.Deserialize<List<string>>(e.TriggersJson) ?? new()
            )).ToList());
    }
}
