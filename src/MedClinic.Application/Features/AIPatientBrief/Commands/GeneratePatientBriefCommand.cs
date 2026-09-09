using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.AIPatientBrief.Commands;

public record GeneratePatientBriefCommand(Guid PatientId, Guid? VisitId = null) : IRequest<Result<PatientBriefDto>>;

public record PatientBriefDto(
    Guid Id,
    string Summary,
    List<string> RecentChanges,
    List<string> PendingItems,
    List<string> Alerts,
    decimal ConfidenceScore,
    bool RequiresDoctorReview,
    DateTime GeneratedAt
);

public class GeneratePatientBriefCommandHandler : IRequestHandler<GeneratePatientBriefCommand, Result<PatientBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIProvider _aiProvider;

    public GeneratePatientBriefCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAIProvider aiProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _aiProvider = aiProvider;
    }

    public async Task<Result<PatientBriefDto>> Handle(GeneratePatientBriefCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.Visits)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId
                && p.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (patient is null) return Result<PatientBriefDto>.Failure("Patient not found.");

        var recentLabs = await _context.LabResults
            .Include(l => l.LabOrder)
            .Where(l => l.LabOrder.PatientId == request.PatientId)
            .OrderByDescending(l => l.ReportedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        var aiResult = await _aiProvider.GeneratePatientBriefAsync(patient, recentLabs, cancellationToken);

        var brief = new PatientBrief
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            VisitId = request.VisitId,
            Summary = aiResult.Summary,
            RecentChangesJson = System.Text.Json.JsonSerializer.Serialize(aiResult.RecentChanges),
            PendingItemsJson = System.Text.Json.JsonSerializer.Serialize(aiResult.PendingItems),
            AlertsJson = System.Text.Json.JsonSerializer.Serialize(aiResult.Alerts),
            AIModel = aiResult.ModelUsed,
            ConfidenceScore = aiResult.ConfidenceScore,
            RequiresDoctorReview = true
        };

        _context.PatientBriefs.Add(brief);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PatientBriefDto>.Success(new PatientBriefDto(
            brief.Id,
            brief.Summary,
            aiResult.RecentChanges,
            aiResult.PendingItems,
            aiResult.Alerts,
            brief.ConfidenceScore,
            true,
            brief.CreatedAt
        ));
    }
}
