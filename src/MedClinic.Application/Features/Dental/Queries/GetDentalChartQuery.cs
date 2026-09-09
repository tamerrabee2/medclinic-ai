using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Dental.Queries;

public record GetDentalChartQuery(Guid PatientId, Guid? VisitId) : IRequest<Result<DentalChartDto>>;

public record DentalChartDto(
    Guid Id,
    Guid PatientId,
    Guid? VisitId,
    string Notes,
    List<ToothRecordDto> Teeth
);

public record ToothRecordDto(Guid Id, int ToothNumber, ToothStatus Status, string? Notes, string SurfaceDataJson);

public class GetDentalChartQueryHandler : IRequestHandler<GetDentalChartQuery, Result<DentalChartDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDentalChartQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<DentalChartDto>> Handle(GetDentalChartQuery request, CancellationToken cancellationToken)
    {
        var chart = await _context.DentalCharts
            .Include(c => c.Teeth.OrderBy(t => t.ToothNumber))
            .FirstOrDefaultAsync(c => c.PatientId == request.PatientId
                && c.VisitId == request.VisitId
                && c.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (chart is null)
        {
            return Result<DentalChartDto>.Success(new DentalChartDto(Guid.Empty, request.PatientId, request.VisitId, string.Empty, new()));
        }

        return Result<DentalChartDto>.Success(new DentalChartDto(
            chart.Id,
            chart.PatientId,
            chart.VisitId,
            chart.Notes,
            chart.Teeth.Select(t => new ToothRecordDto(t.Id, t.ToothNumber, t.Status, t.Notes, t.SurfaceDataJson)).ToList()
        ));
    }
}
