using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class DoctorsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorsController(
        ApplicationDbContext context,
        ITenantContext tenant,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _tenant = tenant;
        _userManager = userManager;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    /// <summary>List doctors in clinic</summary>
    [HttpGet]
    [HasPermission(Permissions.PatientsRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? specialty,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Doctors
            .Where(d => d.ClinicId == clinicId)
            .Include(d => d.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialty))
            query = query.Where(d => d.Specialty.Contains(specialty));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.User.FirstName.Contains(search) ||
                d.User.LastName.Contains(search) ||
                d.Specialty.Contains(search));

        var total = await query.CountAsync(ct);
        var doctors = await query
            .OrderBy(d => d.User.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.UserId,
                FullName = d.User.FirstName + " " + d.User.LastName,
                d.User.Email,
                d.User.AvatarUrl,
                d.Specialty,
                d.Title,
                d.LicenseNumber,
                d.IsAvailable
            })
            .ToListAsync(ct);

        return Success(new { Total = total, Page = page, PageSize = pageSize, Items = doctors });
    }

    /// <summary>Get doctor by ID</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.PatientsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var doctor = await _context.Doctors
            .Where(d => d.Id == id && d.ClinicId == clinicId)
            .Include(d => d.User)
            .Select(d => new
            {
                d.Id, d.UserId,
                FullName = d.User.FirstName + " " + d.User.LastName,
                d.User.Email, d.User.AvatarUrl,
                d.Specialty, d.Title, d.Bio,
                d.LicenseNumber, d.IsAvailable, d.ConsultationFee
            })
            .FirstOrDefaultAsync(ct);

        if (doctor == null) return NotFound("Doctor not found.");
        return Success(doctor);
    }

    /// <summary>Register a user as doctor in this clinic</summary>
    [HttpPost]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return NotFound("User not found.");

        var exists = await _context.Doctors
            .AnyAsync(d => d.UserId == request.UserId && d.ClinicId == clinicId, ct);
        if (exists) return BadRequest("User is already registered as a doctor in this clinic.");

        var doctor = new Doctor
        {
            ClinicId = clinicId,
            UserId = request.UserId,
            Specialty = request.Specialty,
            Title = request.Title,
            Bio = request.Bio,
            LicenseNumber = request.LicenseNumber,
            ConsultationFee = request.ConsultationFee,
            CreatedBy = CurrentUserId
        };

        _context.Doctors.Add(doctor);

        if (!await _userManager.IsInRoleAsync(user, Roles.Doctor))
            await _userManager.AddToRoleAsync(user, Roles.Doctor);

        await _context.SaveChangesAsync(ct);
        return Created(new { doctor.Id, FullName = user.FullName, doctor.Specialty });
    }

    /// <summary>Update doctor profile</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == id && d.ClinicId == clinicId, ct);

        if (doctor == null) return NotFound("Doctor not found.");

        if (request.Specialty != null) doctor.Specialty = request.Specialty;
        if (request.Title != null) doctor.Title = request.Title;
        if (request.Bio != null) doctor.Bio = request.Bio;
        if (request.LicenseNumber != null) doctor.LicenseNumber = request.LicenseNumber;
        if (request.ConsultationFee.HasValue) doctor.ConsultationFee = request.ConsultationFee;
        if (request.IsAvailable.HasValue) doctor.IsAvailable = request.IsAvailable.Value;
        doctor.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Doctor profile updated.");
    }
}

public record CreateDoctorRequest(
    Guid UserId,
    string Specialty,
    string? Title,
    string? Bio,
    string? LicenseNumber,
    decimal? ConsultationFee);

public record UpdateDoctorRequest(
    string? Specialty,
    string? Title,
    string? Bio,
    string? LicenseNumber,
    decimal? ConsultationFee,
    bool? IsAvailable);
