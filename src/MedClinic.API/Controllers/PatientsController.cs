using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class PatientsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public PatientsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    /// <summary>List patients with pagination, search, filtering</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Patients
            .Where(p => p.ClinicId == clinicId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.FirstName.Contains(search) ||
                p.LastName.Contains(search) ||
                (p.Phone != null && p.Phone.Contains(search)) ||
                (p.NationalId != null && p.NationalId.Contains(search)));

        var total = await query.CountAsync(ct);

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.FirstName, p.LastName, p.DateOfBirth,
                p.Gender, p.Phone, p.Email, p.NationalId, p.CreatedAt
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items = patients.Cast<object>().ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>Get patient by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var patient = await _context.Patients
            .Where(p => p.Id == id && p.ClinicId == clinicId)
            .Select(p => new
            {
                p.Id, p.FirstName, p.LastName, p.DateOfBirth, p.Gender,
                p.Phone, p.Email, p.Address, p.NationalId, p.BloodType,
                p.Allergies, p.ChronicConditions, p.Notes, p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (patient == null) return NotFound("Patient not found.");
        return Success(patient);
    }

    /// <summary>Create new patient</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var patient = new Patient
        {
            ClinicId = clinicId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            NationalId = request.NationalId,
            BloodType = request.BloodType,
            Allergies = request.Allergies,
            ChronicConditions = request.ChronicConditions,
            Notes = request.Notes,
            CreatedBy = CurrentUserId
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(ct);

        return Created(new { patient.Id, patient.FirstName, patient.LastName }, "Patient created.");
    }

    /// <summary>Update patient</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (patient == null) return NotFound("Patient not found.");

        patient.FirstName = request.FirstName ?? patient.FirstName;
        patient.LastName = request.LastName ?? patient.LastName;
        patient.Phone = request.Phone ?? patient.Phone;
        patient.Email = request.Email ?? patient.Email;
        patient.Address = request.Address ?? patient.Address;
        patient.Allergies = request.Allergies ?? patient.Allergies;
        patient.ChronicConditions = request.ChronicConditions ?? patient.ChronicConditions;
        patient.Notes = request.Notes ?? patient.Notes;
        patient.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Patient updated.");
    }

    /// <summary>Soft delete patient</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (patient == null) return NotFound("Patient not found.");

        patient.IsDeleted = true;
        patient.DeletedAt = DateTime.UtcNow;
        patient.DeletedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Patient deleted.");
    }
}

public record CreatePatientRequest(
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Phone,
    string? Email,
    string? Address,
    string? NationalId,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? Notes);

public record UpdatePatientRequest(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Email,
    string? Address,
    string? Allergies,
    string? ChronicConditions,
    string? Notes);
