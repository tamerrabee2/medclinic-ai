using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Reports.Queries;

public record GetReportDataQuery(
    DateTime From,
    DateTime To,
    string ReportType
) : IRequest<Result<ReportDataDto>>;

public record ReportDataDto(
    string ReportType,
    DateTime From,
    DateTime To,
    int TotalPatients,
    int TotalAppointments,
    int CompletedVisits,
    int PendingFollowUps,
    decimal TotalRevenue,
    decimal CollectedRevenue,
    decimal PendingRevenue,
    int TotalLabOrders,
    int TotalImagingStudies,
    int AIInteractions,
    List<DailyStatDto> DailyStats
);

public record DailyStatDto(
    DateTime Date,
    int Appointments,
    int Visits,
    decimal Revenue
);

public class GetReportDataQueryHandler : IRequestHandler<GetReportDataQuery, Result<ReportDataDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetReportDataQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ReportDataDto>> Handle(GetReportDataQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.ClinicId!.Value;
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1).AddTicks(-1);

        var totalPatients = await _context.Patients
            .CountAsync(p => p.ClinicId == clinicId && p.CreatedAt >= from && p.CreatedAt <= to, cancellationToken);

        var totalAppointments = await _context.Appointments
            .CountAsync(a => a.ClinicId == clinicId && a.AppointmentDate >= from && a.AppointmentDate <= to, cancellationToken);

        var completedVisits = await _context.Visits
            .CountAsync(v => v.ClinicId == clinicId && v.VisitDate >= from && v.VisitDate <= to, cancellationToken);

        var revenueData = await _context.Invoices
            .Where(i => i.ClinicId == clinicId && i.IssuedDate >= from && i.IssuedDate <= to)
            .GroupBy(i => 1)
            .Select(g => new
            {
                Total = g.Sum(i => i.TotalAmount),
                Collected = g.Sum(i => i.PaidAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalLabs = await _context.LabOrders
            .CountAsync(l => l.ClinicId == clinicId && l.OrderDate >= from && l.OrderDate <= to, cancellationToken);

        var totalImaging = await _context.RadiologyStudies
            .CountAsync(r => r.ClinicId == clinicId && r.StudyDate >= from && r.StudyDate <= to, cancellationToken);

        var aiInteractions = await _context.AIConversations
            .CountAsync(a => a.ClinicId == clinicId && a.CreatedAt >= from && a.CreatedAt <= to, cancellationToken);

        var pendingFollowUps = await _context.FollowUpIntelligences
            .CountAsync(f => f.ClinicId == clinicId && !f.DoctorApproved && !f.DoctorDismissed, cancellationToken);

        var dailyStats = await _context.Appointments
            .Where(a => a.ClinicId == clinicId && a.AppointmentDate >= from && a.AppointmentDate <= to)
            .GroupBy(a => a.AppointmentDate.Date)
            .Select(g => new DailyStatDto(g.Key, g.Count(), 0, 0))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var totalRevenue = revenueData?.Total ?? 0;
        var collected = revenueData?.Collected ?? 0;

        return Result<ReportDataDto>.Success(new ReportDataDto(
            request.ReportType,
            from, to,
            totalPatients,
            totalAppointments,
            completedVisits,
            pendingFollowUps,
            totalRevenue,
            collected,
            totalRevenue - collected,
            totalLabs,
            totalImaging,
            aiInteractions,
            dailyStats
        ));
    }
}
