using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class AppointmentsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public AppointmentsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    /// <summary>List appointments with filters</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Appointments
            .Where(a => a.ClinicId == clinicId)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsQueryable();

        if (doctorId.HasValue) query = query.Where(a => a.DoctorId == doctorId);
        if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId);
        if (from.HasValue) query = query.Where(a => a.ScheduledAt >= from);
        if (to.HasValue) query = query.Where(a => a.ScheduledAt <= to);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id, a.ScheduledAt, a.DurationMinutes, a.Status, a.Type, a.Notes,
                Patient = new { a.Patient.Id, a.Patient.FirstName, a.Patient.LastName },
                Doctor = new { a.Doctor.Id, a.Doctor.UserId }
            })
            .ToListAsync(ct);

        return Success(new { Total = total, Page = page, PageSize = pageSize, Items = items });
    }

    /// <summary>Create appointment</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            Type = request.Type,
            Notes = request.Notes,
            Status = "Scheduled",
            CreatedBy = CurrentUserId
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(ct);

        return Created(new { appointment.Id, appointment.ScheduledAt, appointment.Status });
    }

    /// <summary>Update appointment status</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.ClinicId == clinicId, ct);

        if (appointment == null) return NotFound("Appointment not found.");

        appointment.Status = request.Status;
        if (request.Status == "Cancelled")
            appointment.CancellationReason = request.Reason;
        appointment.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Status updated.");
    }

    /// <summary>Cancel appointment</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.ClinicId == clinicId, ct);

        if (appointment == null) return NotFound("Appointment not found.");

        appointment.Status = "Cancelled";
        appointment.CancellationReason = request.Reason;
        appointment.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Appointment cancelled.");
    }
}

public record CreateAppointmentRequest(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? Type,
    string? Notes);

public record UpdateStatusRequest(string Status, string? Reason);
public record CancelRequest(string? Reason);
