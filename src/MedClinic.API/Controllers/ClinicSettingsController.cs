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
[Route("api/v1/clinic/settings")]
public class ClinicSettingsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;

    public ClinicSettingsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ──────────────────────────────────────────────────────────────────
    // GET
    // ──────────────────────────────────────────────────────────────────

    [HttpGet]
    [HasPermission(Permissions.ClinicsRead)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == ClinicId, ct);

        if (clinic == null) return NotFound("Clinic not found.");

        return Success(new
        {
            clinic.Id,
            clinic.Name,
            clinic.Slug,
            clinic.Phone,
            clinic.Email,
            clinic.Address,
            clinic.City,
            clinic.Country,
            clinic.LogoUrl,
            clinic.Website,
            clinic.Currency,
            clinic.Timezone,
            clinic.WorkingHoursStart,
            clinic.WorkingHoursEnd,
            clinic.WorkingDays,
            clinic.DefaultAppointmentDuration,
            clinic.AllowOnlineBooking,
            clinic.MaxDailyAppointments,
            clinic.TaxRate,
            clinic.InvoicePrefix,
            clinic.CreatedAt
        });
    }

    // ──────────────────────────────────────────────────────────────────
    // UPDATE
    // ──────────────────────────────────────────────────────────────────

    [HttpPut]
    [HasPermission(Permissions.ClinicsManage)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateClinicSettingsRequest request,
        CancellationToken ct)
    {
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == ClinicId, ct);

        if (clinic == null) return NotFound("Clinic not found.");

        if (request.Name    != null) clinic.Name    = request.Name;
        if (request.Phone   != null) clinic.Phone   = request.Phone;
        if (request.Email   != null) clinic.Email   = request.Email;
        if (request.Address != null) clinic.Address = request.Address;
        if (request.City    != null) clinic.City    = request.City;
        if (request.Country != null) clinic.Country = request.Country;
        if (request.Website != null) clinic.Website = request.Website;
        if (request.LogoUrl != null) clinic.LogoUrl = request.LogoUrl;
        if (request.Currency != null) clinic.Currency = request.Currency;
        if (request.Timezone != null) clinic.Timezone = request.Timezone;
        if (request.WorkingHoursStart.HasValue) clinic.WorkingHoursStart = request.WorkingHoursStart;
        if (request.WorkingHoursEnd.HasValue)   clinic.WorkingHoursEnd   = request.WorkingHoursEnd;
        if (request.WorkingDays != null) clinic.WorkingDays = request.WorkingDays;
        if (request.DefaultAppointmentDuration.HasValue)
            clinic.DefaultAppointmentDuration = request.DefaultAppointmentDuration.Value;
        if (request.AllowOnlineBooking.HasValue)
            clinic.AllowOnlineBooking = request.AllowOnlineBooking.Value;
        if (request.MaxDailyAppointments.HasValue)
            clinic.MaxDailyAppointments = request.MaxDailyAppointments.Value;
        if (request.TaxRate.HasValue)       clinic.TaxRate       = request.TaxRate.Value;
        if (request.InvoicePrefix != null) clinic.InvoicePrefix = request.InvoicePrefix;

        clinic.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Clinic settings updated.");
    }

    // ──────────────────────────────────────────────────────────────────
    // STAFF MANAGEMENT
    // ──────────────────────────────────────────────────────────────────

    [HttpGet("staff")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetStaff(CancellationToken ct)
    {
        var clinicId = ClinicId;
        var members  = await _context.ClinicMembers
            .Where(m => m.ClinicId == clinicId)
            .Include(m => m.User)
            .OrderBy(m => m.Role)
            .Select(m => new
            {
                m.Id,
                m.UserId,
                m.Role,
                m.IsActive,
                m.JoinedAt,
                User = new
                {
                    m.User.Id,
                    m.User.FirstName,
                    m.User.LastName,
                    m.User.Email,
                    m.User.PhoneNumber
                }
            })
            .ToListAsync(ct);

        return Success(members);
    }

    [HttpPatch("staff/{memberId:guid}/role")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> UpdateStaffRole(
        Guid memberId,
        [FromBody] UpdateStaffRoleRequest request,
        CancellationToken ct)
    {
        var member = await _context.ClinicMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.ClinicId == ClinicId, ct);

        if (member == null) return NotFound("Staff member not found.");
        if (!Roles.All.Contains(request.Role))
            return BadRequest($"Invalid role '{request.Role}'.");

        member.Role      = request.Role;
        member.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, $"Role updated to {request.Role}.");
    }

    [HttpPatch("staff/{memberId:guid}/deactivate")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> DeactivateStaff(Guid memberId, CancellationToken ct)
    {
        var member = await _context.ClinicMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.ClinicId == ClinicId, ct);

        if (member == null) return NotFound("Staff member not found.");

        member.IsActive  = false;
        member.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Staff member deactivated.");
    }
}

// ── DTOs ────────────────────────────────────────────────────────────────

public record UpdateClinicSettingsRequest(
    string?   Name,
    string?   Phone,
    string?   Email,
    string?   Address,
    string?   City,
    string?   Country,
    string?   Website,
    string?   LogoUrl,
    string?   Currency,
    string?   Timezone,
    TimeOnly? WorkingHoursStart,
    TimeOnly? WorkingHoursEnd,
    string?   WorkingDays,            // e.g. "Mon,Tue,Wed,Thu,Fri"
    int?      DefaultAppointmentDuration,
    bool?     AllowOnlineBooking,
    int?      MaxDailyAppointments,
    decimal?  TaxRate,
    string?   InvoicePrefix);

public record UpdateStaffRoleRequest(string Role);
