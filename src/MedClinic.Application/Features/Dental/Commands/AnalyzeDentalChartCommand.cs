using System.Text.Json;
using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Dental.Commands;

public record AnalyzeDentalChartCommand(Guid PatientId, Guid? VisitId) : IRequest<Result<DentalChartAnalysisDto>>;

public record DentalChartAnalysisDto(
    List<string> Findings,
    List<string> RecommendedActions,
    decimal Confidence,
    bool RequiresDoctorReview
);

public class AnalyzeDentalChartCommandHandler : IRequestHandler<AnalyzeDentalChartCommand, Result<DentalChartAnalysisDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AnalyzeDentalChartCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<DentalChartAnalysisDto>> Handle(AnalyzeDentalChartCommand request, CancellationToken cancellationToken)
    {
        var chart = await _context.DentalCharts
            .Include(c => c.Teeth)
            .FirstOrDefaultAsync(c => c.PatientId == request.PatientId
                && c.VisitId == request.VisitId
                && c.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (chart is null)
            return Result<DentalChartAnalysisDto>.Failure("Dental chart not found.");

        var findings = new List<string>();
        var actions = new List<string>();

        var caries = chart.Teeth.Where(t => t.Status == Domain.Entities.ToothStatus.Caries).Select(t => t.ToothNumber).ToList();
        var missing = chart.Teeth.Where(t => t.Status == Domain.Entities.ToothStatus.Missing).Select(t => t.ToothNumber).ToList();
        var rootCanals = chart.Teeth.Where(t => t.Status == Domain.Entities.ToothStatus.RootCanal).Select(t => t.ToothNumber).ToList();

        if (caries.Any())
        {
            findings.Add($"Dental caries detected in teeth: {string.Join(", ", caries)}.");
            actions.Add("Consider restorative treatment and caries risk assessment.");
        }

        if (missing.Any())
        {
            findings.Add($"Missing teeth recorded: {string.Join(", ", missing)}.");
            actions.Add("Evaluate prosthetic replacement options if clinically appropriate.");
        }

        if (rootCanals.Any())
        {
            findings.Add($"Root canal treated teeth present: {string.Join(", ", rootCanals)}.");
            actions.Add("Monitor endodontically treated teeth during follow-up.");
        }

        if (!findings.Any())
        {
            findings.Add("No major abnormal dental findings recorded in current chart.");
            actions.Add("Continue routine preventive dental follow-up.");
        }

        return Result<DentalChartAnalysisDto>.Success(new DentalChartAnalysisDto(findings, actions, 0.86m, true));
    }
}
