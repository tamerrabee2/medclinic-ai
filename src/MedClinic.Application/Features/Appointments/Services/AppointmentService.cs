using MedClinic.Application.Features.Appointments.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Appointments.Services;

public class AppointmentService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public AppointmentService(IApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<AppointmentPagedResult<Appointment>> GetAppointmentsAsync(AppointmentListQuery query, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId;
        var q = _context.Appointments
            .Where(a => a.ClinicId == clinicId && !a.IsDeleted)
            .AsQueryable();

        if (query.DoctorId.HasValue)
            q = q.Where(a => a.DoctorId == query.DoctorId.Value);

        if (query.PatientId.HasValue)
            q = q.Where(a => a.PatientId == query.PatientId.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(a => a.ScheduledAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new AppointmentPagedResult<Appointment>(items, total, query.Page, query.PageSize);
    }

    public async Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId ?? Guid.NewGuid();
        var endTime = request.ScheduledAt.AddMinutes(request.DurationMinutes);

        var conflict = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == request.DoctorId &&
            a.ClinicId == clinicId &&
            !a.IsDeleted &&
            a.Status != AppointmentStatus.Cancelled &&
            a.ScheduledAt < endTime &&
            a.ScheduledAt.AddMinutes(a.DurationMinutes) > request.ScheduledAt, ct);

        if (conflict)
            throw new InvalidOperationException("Doctor is already booked at that time slot");

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            Type = request.Type,
            Notes = request.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(ct);
        return appointment;
    }

    public async Task CancelAppointmentAsync(Guid id, string? reason = null, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId;
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.ClinicId == clinicId && !a.IsDeleted, ct);

        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with id '{id}' was not found.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = reason;
        appointment.CancelledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    public async Task CompleteAppointmentAsync(Guid id, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId;
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.ClinicId == clinicId && !a.IsDeleted, ct);

        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with id '{id}' was not found.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}
