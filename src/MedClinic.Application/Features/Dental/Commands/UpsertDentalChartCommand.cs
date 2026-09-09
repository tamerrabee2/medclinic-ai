using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Dental.Commands;

public record UpsertDentalChartCommand(
    Guid PatientId,
    Guid? VisitId,
    string Notes,
    List<ToothRecordInput> Teeth
) : IRequest<Result<Guid>>;

public record ToothRecordInput(int ToothNumber, ToothStatus Status, string? Notes, string SurfaceDataJson);

public class UpsertDentalChartCommandHandler : IRequestHandler<UpsertDentalChartCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpsertDentalChartCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(UpsertDentalChartCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.ClinicId!.Value;

        var chart = await _context.DentalCharts
            .Include(c => c.Teeth)
            .FirstOrDefaultAsync(c => c.PatientId == request.PatientId && c.VisitId == request.VisitId && c.ClinicId == clinicId, cancellationToken);

        if (chart is null)
        {
            chart = new DentalChart
            {
                ClinicId = clinicId,
                PatientId = request.PatientId,
                VisitId = request.VisitId,
                Notes = request.Notes
            };
            _context.DentalCharts.Add(chart);
        }
        else
        {
            chart.Notes = request.Notes;
            _context.ToothRecords.RemoveRange(chart.Teeth);
            chart.Teeth.Clear();
        }

        foreach (var tooth in request.Teeth)
        {
            chart.Teeth.Add(new ToothRecord
            {
                ToothNumber = tooth.ToothNumber,
                Status = tooth.Status,
                Notes = tooth.Notes,
                SurfaceDataJson = string.IsNullOrWhiteSpace(tooth.SurfaceDataJson) ? "{}" : tooth.SurfaceDataJson
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(chart.Id);
    }
}
