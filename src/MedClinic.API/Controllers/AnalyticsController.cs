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
[Route("api/v1/analytics")]
public class AnalyticsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;

    public AnalyticsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ──────────────────────────────────────────────────────────────────
    // APPOINTMENT TRENDS
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Appointment volume trends over time</summary>
    [HttpGet("appointments")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetAppointmentTrends(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string groupBy = "day",
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-3);
        var end      = to   ?? DateTime.UtcNow;

        var data = await _context.Appointments
            .Where(a => a.ClinicId == clinicId &&
                        a.ScheduledAt >= start  &&
                        a.ScheduledAt <= end)
            .Select(a => new { a.ScheduledAt, a.Status })
            .ToListAsync(ct);

        var grouped = groupBy.ToLower() == "month"
            ? data.GroupBy(a => new { a.ScheduledAt.Year, a.ScheduledAt.Month })
                  .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                  .Select(g => new
                  {
                      Period      = $"{g.Key.Year}-{g.Key.Month:D2}",
                      Total       = g.Count(),
                      Completed   = g.Count(a => a.Status == AppointmentStatus.Completed),
                      Cancelled   = g.Count(a => a.Status == AppointmentStatus.Cancelled),
                      NoShow      = g.Count(a => a.Status == AppointmentStatus.NoShow)
                  })
            : data.GroupBy(a => a.ScheduledAt.Date)
                  .OrderBy(g => g.Key)
                  .Select(g => new
                  {
                      Period      = g.Key.ToString("yyyy-MM-dd"),
                      Total       = g.Count(),
                      Completed   = g.Count(a => a.Status == AppointmentStatus.Completed),
                      Cancelled   = g.Count(a => a.Status == AppointmentStatus.Cancelled),
                      NoShow      = g.Count(a => a.Status == AppointmentStatus.NoShow)
                  });

        return Success(new { GroupBy = groupBy, From = start, To = end, Data = grouped });
    }

    // ──────────────────────────────────────────────────────────────────
    // PATIENT GROWTH
    // ──────────────────────────────────────────────────────────────────

    /// <summary>New patient registrations over time + cumulative total</summary>
    [HttpGet("patient-growth")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetPatientGrowth(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string groupBy = "month",
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-12);
        var end      = to   ?? DateTime.UtcNow;

        var patients = await _context.Patients
            .Where(p => p.ClinicId == clinicId &&
                        p.CreatedAt >= start   &&
                        p.CreatedAt <= end)
            .Select(p => new { p.CreatedAt })
            .ToListAsync(ct);

        var grouped = groupBy.ToLower() == "day"
            ? patients.GroupBy(p => p.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Period = g.Key.ToString("yyyy-MM-dd"), NewPatients = g.Count() })
            : patients.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Period = $"{g.Key.Year}-{g.Key.Month:D2}", NewPatients = g.Count() });

        // Add cumulative
        var list = grouped.ToList();
        var totalBefore = await _context.Patients
            .CountAsync(p => p.ClinicId == clinicId && p.CreatedAt < start, ct);

        var cumulative = 0;
        var result = list.Select(x =>
        {
            cumulative += x.NewPatients;
            return new { x.Period, x.NewPatients, Cumulative = totalBefore + cumulative };
        });

        return Success(new { GroupBy = groupBy, From = start, To = end, Data = result });
    }

    // ──────────────────────────────────────────────────────────────────
    // DOCTOR PERFORMANCE
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Doctor performance: appointments, visits, prescriptions per doctor</summary>
    [HttpGet("doctor-performance")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetDoctorPerformance(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-1);
        var end      = to   ?? DateTime.UtcNow;

        var doctors = await _context.Doctors
            .Where(d => d.ClinicId == clinicId)
            .Include(d => d.User)
            .Select(d => new
            {
                d.Id,
                Name      = d.User.FirstName + " " + d.User.LastName,
                d.Specialty,
                Appointments = d.Appointments
                    .Count(a => a.ScheduledAt >= start && a.ScheduledAt <= end),
                CompletedAppts = d.Appointments
                    .Count(a => a.ScheduledAt >= start && a.ScheduledAt <= end
                             && a.Status == AppointmentStatus.Completed),
                Visits = d.Visits
                    .Count(v => v.VisitDate >= start && v.VisitDate <= end)
            })
            .OrderByDescending(d => d.Appointments)
            .ToListAsync(ct);

        return Success(new { From = start, To = end, Doctors = doctors });
    }

    // ──────────────────────────────────────────────────────────────────
    // APPOINTMENT CANCELLATION ANALYSIS
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Cancellation + no-show rates by doctor</summary>
    [HttpGet("cancellation-rates")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetCancellationRates(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-3);
        var end      = to   ?? DateTime.UtcNow;

        var appts = await _context.Appointments
            .Where(a => a.ClinicId == clinicId &&
                        a.ScheduledAt >= start  &&
                        a.ScheduledAt <= end)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Select(a => new
            {
                DoctorId   = a.Doctor.Id,
                DoctorName = a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                a.Status
            })
            .ToListAsync(ct);

        var totalAll       = appts.Count;
        var totalCancelled = appts.Count(a => a.Status == AppointmentStatus.Cancelled);
        var totalNoShow    = appts.Count(a => a.Status == AppointmentStatus.NoShow);

        var byDoctor = appts
            .GroupBy(a => new { a.DoctorId, a.DoctorName })
            .Select(g =>
            {
                var total     = g.Count();
                var cancelled = g.Count(a => a.Status == AppointmentStatus.Cancelled);
                var noShow    = g.Count(a => a.Status == AppointmentStatus.NoShow);
                return new
                {
                    DoctorId         = g.Key.DoctorId,
                    DoctorName       = g.Key.DoctorName,
                    Total            = total,
                    Cancelled        = cancelled,
                    NoShow           = noShow,
                    CancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 2) : 0m,
                    NoShowRate       = total > 0 ? Math.Round((decimal)noShow    / total * 100, 2) : 0m
                };
            })
            .OrderByDescending(d => d.CancellationRate);

        return Success(new
        {
            From        = start,
            To          = end,
            Overall = new
            {
                Total            = totalAll,
                Cancelled        = totalCancelled,
                NoShow           = totalNoShow,
                CancellationRate = totalAll > 0 ? Math.Round((decimal)totalCancelled / totalAll * 100, 2) : 0m,
                NoShowRate       = totalAll > 0 ? Math.Round((decimal)totalNoShow    / totalAll * 100, 2) : 0m
            },
            ByDoctor = byDoctor
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // PATIENT DEMOGRAPHICS
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Patient demographics: age groups + gender distribution</summary>
    [HttpGet("demographics")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetDemographics(CancellationToken ct)
    {
        var clinicId = ClinicId;
        var now      = DateTime.UtcNow;

        var patients = await _context.Patients
            .Where(p => p.ClinicId == clinicId)
            .Select(p => new { p.DateOfBirth, p.Gender })
            .ToListAsync(ct);

        // Age groups
        var ageGroups = patients
            .Select(p => (int)((now - p.DateOfBirth).TotalDays / 365.25))
            .GroupBy(age => age switch
            {
                < 13  => "0-12",
                < 18  => "13-17",
                < 30  => "18-29",
                < 45  => "30-44",
                < 60  => "45-59",
                < 75  => "60-74",
                _     => "75+"
            })
            .Select(g => new { AgeGroup = g.Key, Count = g.Count() })
            .OrderBy(g => g.AgeGroup);

        // Gender
        var genderDist = patients
            .GroupBy(p => p.Gender ?? "Unknown")
            .Select(g => new { Gender = g.Key, Count = g.Count() });

        var total = patients.Count;

        return Success(new
        {
            TotalPatients = total,
            AgeGroups     = ageGroups,
            GenderDistribution = genderDist.Select(g => new
            {
                g.Gender,
                g.Count,
                Percentage = total > 0 ? Math.Round((decimal)g.Count / total * 100, 2) : 0m
            })
        });
    }
}
