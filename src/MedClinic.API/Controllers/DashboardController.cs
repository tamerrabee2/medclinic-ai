using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
[Route("api/v1/dashboard")]
public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;

    public DashboardController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ──────────────────────────────────────────────────────────────────
    // MAIN OVERVIEW
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Main dashboard — all KPIs in one call</summary>
    [HttpGet("overview")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var clinicId   = ClinicId;
        var now        = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd   = todayStart.AddDays(1);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevStart  = monthStart.AddMonths(-1);
        var prevEnd    = monthStart;

        // ── Patients ──
        var totalPatients   = await _context.Patients.CountAsync(p => p.ClinicId == clinicId, ct);
        var newPatientsMonth = await _context.Patients
            .CountAsync(p => p.ClinicId == clinicId && p.CreatedAt >= monthStart, ct);
        var newPatientsToday = await _context.Patients
            .CountAsync(p => p.ClinicId == clinicId &&
                             p.CreatedAt >= todayStart && p.CreatedAt < todayEnd, ct);

        // ── Appointments ──
        var appointmentsToday = await _context.Appointments
            .CountAsync(a => a.ClinicId == clinicId &&
                             a.ScheduledAt >= todayStart &&
                             a.ScheduledAt < todayEnd, ct);
        var appointmentsMonth = await _context.Appointments
            .CountAsync(a => a.ClinicId == clinicId && a.ScheduledAt >= monthStart, ct);
        var pendingAppointments = await _context.Appointments
            .CountAsync(a => a.ClinicId == clinicId &&
                             a.Status == AppointmentStatus.Pending &&
                             a.ScheduledAt >= now, ct);

        // ── Visits ──
        var activeVisits = await _context.Visits
            .CountAsync(v => v.ClinicId == clinicId &&
                             v.Status == VisitStatus.InProgress, ct);
        var visitsToday = await _context.Visits
            .CountAsync(v => v.ClinicId == clinicId &&
                             v.VisitDate >= todayStart &&
                             v.VisitDate < todayEnd, ct);

        // ── Lab ──
        var pendingLab = await _context.LabOrders
            .CountAsync(o => o.ClinicId == clinicId &&
                             (o.Status == LabOrderStatus.Pending ||
                              o.Status == LabOrderStatus.Collected ||
                              o.Status == LabOrderStatus.Processing), ct);

        // ── Radiology ──
        var pendingRadiology = await _context.RadiologyStudies
            .CountAsync(s => s.ClinicId == clinicId &&
                             (s.Status == RadiologyStudyStatus.Pending ||
                              s.Status == RadiologyStudyStatus.InProgress), ct);

        // ── Billing ──
        var revenueToday = await _context.Payments
            .Where(p => p.Invoice.ClinicId == clinicId &&
                        p.PaidAt >= todayStart &&
                        p.PaidAt < todayEnd    &&
                        p.Status == PaymentStatus.Completed &&
                        p.Amount > 0)
            .SumAsync(p => p.Amount, ct);

        var revenueMonth = await _context.Payments
            .Where(p => p.Invoice.ClinicId == clinicId &&
                        p.PaidAt >= monthStart &&
                        p.Status == PaymentStatus.Completed &&
                        p.Amount > 0)
            .SumAsync(p => p.Amount, ct);

        var revenuePrevMonth = await _context.Payments
            .Where(p => p.Invoice.ClinicId == clinicId &&
                        p.PaidAt >= prevStart  &&
                        p.PaidAt < prevEnd     &&
                        p.Status == PaymentStatus.Completed &&
                        p.Amount > 0)
            .SumAsync(p => p.Amount, ct);

        var outstandingBalance = await _context.Invoices
            .Where(i => i.ClinicId == clinicId &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.Status != InvoiceStatus.Draft      &&
                        i.Status != InvoiceStatus.Paid)
            .SumAsync(i => i.TotalAmount - i.PaidAmount, ct);

        var overdueInvoices = await _context.Invoices
            .CountAsync(i => i.ClinicId == clinicId &&
                             i.Status == InvoiceStatus.Overdue, ct);

        // ── Notifications ──
        var myUnread = await _context.Notifications
            .CountAsync(n => n.UserId == CurrentUserId && n.ClinicId == clinicId && !n.IsRead, ct);

        // ── Staff ──
        var totalDoctors = await _context.Doctors
            .CountAsync(d => d.ClinicId == clinicId, ct);
        var totalStaff = await _context.ClinicMembers
            .CountAsync(m => m.ClinicId == clinicId, ct);

        // ── Revenue growth ──
        var revenueGrowth = revenuePrevMonth > 0
            ? Math.Round((revenueMonth - revenuePrevMonth) / revenuePrevMonth * 100, 2)
            : 0m;

        return Success(new
        {
            GeneratedAt = now,
            Patients = new
            {
                Total       = totalPatients,
                NewThisMonth = newPatientsMonth,
                NewToday    = newPatientsToday
            },
            Appointments = new
            {
                Today   = appointmentsToday,
                Month   = appointmentsMonth,
                Pending = pendingAppointments
            },
            Visits = new
            {
                Active = activeVisits,
                Today  = visitsToday
            },
            Lab = new { PendingOrders = pendingLab },
            Radiology = new { PendingStudies = pendingRadiology },
            Billing = new
            {
                RevenueToday       = revenueToday,
                RevenueThisMonth   = revenueMonth,
                RevenuePrevMonth   = revenuePrevMonth,
                RevenueGrowthPct   = revenueGrowth,
                OutstandingBalance = Math.Max(0, outstandingBalance),
                OverdueInvoices    = overdueInvoices
            },
            Staff = new { Doctors = totalDoctors, Total = totalStaff },
            Notifications = new { Unread = myUnread }
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // TODAY’S SCHEDULE
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Today’s appointment schedule grouped by doctor</summary>
    [HttpGet("today-schedule")]
    [HasPermission(Permissions.AppointmentsRead)]
    public async Task<IActionResult> GetTodaySchedule(CancellationToken ct)
    {
        var clinicId   = ClinicId;
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1);

        var appointments = await _context.Appointments
            .Where(a => a.ClinicId == clinicId &&
                        a.ScheduledAt >= todayStart &&
                        a.ScheduledAt < todayEnd    &&
                        a.Status != AppointmentStatus.Cancelled)
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .OrderBy(a => a.ScheduledAt)
            .Select(a => new
            {
                a.Id,
                a.ScheduledAt,
                a.DurationMinutes,
                a.Status,
                a.ReasonForVisit,
                Patient = new { a.Patient.Id, a.Patient.FirstName, a.Patient.LastName, a.Patient.Phone },
                Doctor  = new
                {
                    a.Doctor.Id,
                    Name = a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                    a.Doctor.Specialty
                }
            })
            .ToListAsync(ct);

        var byDoctor = appointments
            .GroupBy(a => new { a.Doctor.Id, a.Doctor.Name, a.Doctor.Specialty })
            .Select(g => new
            {
                DoctorId   = g.Key.Id,
                DoctorName = g.Key.Name,
                Specialty  = g.Key.Specialty,
                Count      = g.Count(),
                Appointments = g.OrderBy(a => a.ScheduledAt)
            });

        return Success(new
        {
            Date         = todayStart.ToString("yyyy-MM-dd"),
            TotalCount   = appointments.Count,
            ByDoctor     = byDoctor
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // RECENT ACTIVITY
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Recent activity feed across all entities</summary>
    [HttpGet("activity")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetRecentActivity(
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 5, 50);
        var clinicId = ClinicId;
        var since    = DateTime.UtcNow.AddDays(-7);

        // Pull recent audit logs as activity feed
        var activity = await _context.AuditLogs
            .Where(a => a.ClinicId == clinicId && a.CreatedAt >= since)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.EntityName,
                a.EntityId,
                a.Action,
                a.UserName,
                a.CreatedAt
            })
            .ToListAsync(ct);

        return Success(new { Items = activity, Count = activity.Count });
    }
}
