using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
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
                cm.Clinic.Id,
                cm.Clinic.Name,
                cm.Clinic.Slug,
                cm.Clinic.LogoUrl,
                cm.Clinic.IsActive,
                MemberRole = cm.Role
            })
            .ToListAsync(ct);

        return Success(clinics);
    }

    /// <summary>Get clinic by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinic = await _context.Clinics
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.Name, c.Slug, c.Email, c.Phone,
                c.Address, c.City, c.Country, c.LogoUrl, c.IsActive,
                c.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (clinic == null) return NotFound("Clinic not found.");
        return Success(clinic);
    }

    /// <summary>Create a new clinic</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClinicRequest request, CancellationToken ct)
    {
        var slug = request.Name.ToLower().Replace(" ", "-");
        var slugExists = await _context.Clinics.AnyAsync(c => c.Slug == slug, ct);
        if (slugExists)
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

        var member = new ClinicMember
        {
            ClinicId = clinic.Id,
            UserId = CurrentUserId,
            Role = "ClinicAdmin",
            CreatedBy = CurrentUserId
        };

        _context.ClinicMembers.Add(member);
        await _context.SaveChangesAsync(ct);

        return Created(new { clinic.Id, clinic.Name, clinic.Slug }, "Clinic created successfully.");
    }
}

public record CreateClinicRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country);
