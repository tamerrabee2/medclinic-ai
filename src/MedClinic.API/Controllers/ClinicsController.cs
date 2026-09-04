using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class ClinicsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public ClinicsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    /// <summary>Get all clinics for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var clinics = await _context.ClinicMembers
            .Where(cm => cm.UserId == CurrentUserId && !cm.IsDeleted)
            .Include(cm => cm.Clinic)
            .Select(cm => new
            {
                cm.Clinic.Id, cm.Clinic.Name, cm.Clinic.Slug,
                cm.Clinic.LogoUrl, cm.Clinic.IsActive,
                MemberRole = cm.Role
            })
            .ToListAsync(ct);

        return Success(clinics);
    }

    /// <summary>Get clinic by ID</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.ClinicsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinic = await _context.Clinics
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new
            {
                c.Id, c.Name, c.Slug, c.Email,
                c.Phone, c.Address, c.City, c.Country,
                c.LogoUrl, c.IsActive, c.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (clinic == null) return NotFound("Clinic not found.");
        return Success(clinic);
    }

    /// <summary>Create new clinic</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClinicRequest request, CancellationToken ct)
    {
        var slug = request.Name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace(",", "");

        if (await _context.Clinics.AnyAsync(c => c.Slug == slug, ct))
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        var clinic = new Clinic
        {
            Name = request.Name,
            Slug = slug,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            CreatedBy = CurrentUserId
        };

        _context.Clinics.Add(clinic);

        _context.ClinicMembers.Add(new ClinicMember
        {
            ClinicId = clinic.Id,
            UserId = CurrentUserId,
            Role = Roles.ClinicAdmin,
            JoinedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId
        });

        await _context.SaveChangesAsync(ct);
        return Created(new { clinic.Id, clinic.Name, clinic.Slug }, "Clinic created.");
    }

    /// <summary>Update clinic info</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.ClinicsManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClinicRequest request, CancellationToken ct)
    {
        var clinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

        if (clinic == null) return NotFound("Clinic not found.");

        if (request.Name != null) clinic.Name = request.Name;
        if (request.Email != null) clinic.Email = request.Email;
        if (request.Phone != null) clinic.Phone = request.Phone;
        if (request.Address != null) clinic.Address = request.Address;
        if (request.City != null) clinic.City = request.City;
        if (request.Country != null) clinic.Country = request.Country;
        if (request.LogoUrl != null) clinic.LogoUrl = request.LogoUrl;
        clinic.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Clinic updated.");
    }

    /// <summary>Get clinic stats summary</summary>
    [HttpGet("{id:guid}/stats")]
    [HasPermission(Permissions.ClinicsRead)]
    public async Task<IActionResult> GetStats(Guid id, CancellationToken ct)
    {
        var stats = new
        {
            TotalPatients    = await _context.Patients.CountAsync(p => p.ClinicId == id, ct),
            TotalDoctors     = await _context.Doctors.CountAsync(d => d.ClinicId == id, ct),
            TotalMembers     = await _context.ClinicMembers.CountAsync(cm => cm.ClinicId == id && !cm.IsDeleted, ct),
            TodayAppointments = await _context.Appointments.CountAsync(a =>
                a.ClinicId == id &&
                a.ScheduledAt.Date == DateTime.UtcNow.Date &&
                a.Status != "Cancelled", ct),
            PendingAppointments = await _context.Appointments.CountAsync(a =>
                a.ClinicId == id && a.Status == "Scheduled", ct)
        };

        return Success(stats);
    }
}

public record CreateClinicRequest(
    string Name, string? Email, string? Phone,
    string? Address, string? City, string? Country);

public record UpdateClinicRequest(
    string? Name, string? Email, string? Phone,
    string? Address, string? City, string? Country, string? LogoUrl);
