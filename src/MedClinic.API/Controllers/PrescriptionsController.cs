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
public class PrescriptionsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public PrescriptionsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ─────────────────────────────────────────────────────────────────────
    // LIST
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>List prescriptions with optional filters</summary>
    [HttpGet]
    [HasPermission(Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   patientId,
        [FromQuery] Guid?   doctorId,
        [FromQuery] Guid?   visitId,
        [FromQuery] bool?   isSigned,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        CancellationToken   ct       = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Prescriptions
            .Where(p => p.ClinicId == clinicId)
            .Include(p => p.Patient)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Items)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(p => p.PatientId == patientId);
        if (doctorId.HasValue)  query = query.Where(p => p.DoctorId  == doctorId);
        if (visitId.HasValue)   query = query.Where(p => p.VisitId   == visitId);
        if (isSigned.HasValue)  query = query.Where(p => p.IsSigned  == isSigned);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.IssuedAt,
                p.ExpiresAt,
                p.IsSigned,
                p.PdfUrl,
                p.DiagnosisSummary,
                Patient  = new { p.Patient.Id, p.Patient.FirstName, p.Patient.LastName },
                Doctor   = new
                {
                    p.Doctor.Id,
                    Name = p.Doctor.User.FirstName + " " + p.Doctor.User.LastName,
                    p.Doctor.Specialty
                },
                ItemCount = p.Items.Count
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items      = items.Cast<object>().ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Get prescription with all items</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var rx = await _context.Prescriptions
            .Where(p => p.Id == id && p.ClinicId == clinicId)
            .Include(p => p.Patient)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(ct);

        if (rx == null) return NotFound("Prescription not found.");

        return Success(new
        {
            rx.Id, rx.IssuedAt, rx.ExpiresAt,
            rx.IsSigned, rx.PdfUrl,
            rx.DiagnosisSummary, rx.Notes,
            rx.VisitId,
            Patient = new { rx.Patient.Id, rx.Patient.FirstName, rx.Patient.LastName, rx.Patient.DateOfBirth },
            Doctor  = new
            {
                rx.Doctor.Id,
                Name = rx.Doctor.User.FirstName + " " + rx.Doctor.User.LastName,
                rx.Doctor.Specialty, rx.Doctor.Title, rx.Doctor.LicenseNumber
            },
            Items = rx.Items.Select(i => new
            {
                i.Id,
                i.MedicineName,
                i.Dosage,
                i.Frequency,
                i.DurationDays,
                i.Route,
                i.Instructions,
                i.Quantity
            })
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Create a new prescription (with optional items)</summary>
    [HttpPost]
    [HasPermission(Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePrescriptionRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        // Validate patient & doctor
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId && p.ClinicId == clinicId, ct);
        if (!patientExists) return NotFound("Patient not found.");

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId && d.ClinicId == clinicId, ct);
        if (!doctorExists) return NotFound("Doctor not found.");

        // Validate visit belongs to same clinic/patient if provided
        if (request.VisitId.HasValue)
        {
            var visitOk = await _context.Visits.AnyAsync(v =>
                v.Id        == request.VisitId &&
                v.ClinicId  == clinicId &&
                v.PatientId == request.PatientId, ct);
            if (!visitOk) return BadRequest("Visit does not match patient or clinic.");
        }

        var rx = new Prescription
        {
            ClinicId         = clinicId,
            PatientId        = request.PatientId,
            DoctorId         = request.DoctorId,
            VisitId          = request.VisitId,
            IssuedAt         = DateTime.UtcNow,
            ExpiresAt        = request.ExpiresAt,
            DiagnosisSummary = request.DiagnosisSummary,
            Notes            = request.Notes,
            CreatedBy        = CurrentUserId
        };

        if (request.Items?.Count > 0)
            rx.Items = request.Items.Select(i => MapItem(i)).ToList();

        _context.Prescriptions.Add(rx);
        await _context.SaveChangesAsync(ct);

        return Created(new { rx.Id, rx.IssuedAt, ItemCount = rx.Items.Count }, "Prescription created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Update prescription header (not items — use items endpoints)</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePrescriptionRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var rx = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (rx == null) return NotFound("Prescription not found.");
        if (rx.IsSigned)  return BadRequest("Cannot edit a signed prescription.");

        if (request.DiagnosisSummary != null) rx.DiagnosisSummary = request.DiagnosisSummary;
        if (request.Notes            != null) rx.Notes            = request.Notes;
        if (request.ExpiresAt.HasValue)       rx.ExpiresAt        = request.ExpiresAt;
        rx.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Prescription updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // SIGN
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Doctor signs the prescription (locks it)</summary>
    [HttpPost("{id:guid}/sign")]
    [HasPermission(Permissions.PrescriptionsSign)]
    public async Task<IActionResult> Sign(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var rx = await _context.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (rx == null)    return NotFound("Prescription not found.");
        if (rx.IsSigned)   return BadRequest("Prescription already signed.");
        if (!rx.Items.Any()) return BadRequest("Cannot sign an empty prescription.");

        rx.IsSigned  = true;
        rx.UpdatedBy = CurrentUserId;
        // PdfUrl will be generated in Phase 5 (document generation)

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Prescription signed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ITEMS — ADD
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Add a medication item to a prescription</summary>
    [HttpPost("{id:guid}/items")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] PrescriptionItemRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var rx = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (rx == null)   return NotFound("Prescription not found.");
        if (rx.IsSigned)  return BadRequest("Cannot add items to a signed prescription.");

        var item = MapItem(request);
        item.PrescriptionId = rx.Id;

        _context.PrescriptionItems.Add(item);
        await _context.SaveChangesAsync(ct);

        return Created(new { item.Id, item.MedicineName }, "Item added.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ITEMS — UPDATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Update a medication item</summary>
    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> UpdateItem(
        Guid id,
        Guid itemId,
        [FromBody] PrescriptionItemRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var rx = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (rx == null)  return NotFound("Prescription not found.");
        if (rx.IsSigned) return BadRequest("Cannot edit a signed prescription.");

        var item = await _context.PrescriptionItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.PrescriptionId == id, ct);

        if (item == null) return NotFound("Item not found.");

        item.MedicineName  = request.MedicineName;
        item.Dosage        = request.Dosage;
        item.Frequency     = request.Frequency;
        item.DurationDays  = request.DurationDays;
        item.Route         = request.Route;
        item.Instructions  = request.Instructions;
        item.Quantity      = request.Quantity;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Item updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ITEMS — DELETE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Remove a medication item from prescription</summary>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> DeleteItem(
        Guid id,
        Guid itemId,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var rx = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, ct);

        if (rx == null)  return NotFound("Prescription not found.");
        if (rx.IsSigned) return BadRequest("Cannot remove items from a signed prescription.");

        var item = await _context.PrescriptionItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.PrescriptionId == id, ct);

        if (item == null) return NotFound("Item not found.");

        _context.PrescriptionItems.Remove(item);
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Item removed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────────────

    private static PrescriptionItem MapItem(PrescriptionItemRequest r) => new()
    {
        MedicineName = r.MedicineName,
        Dosage       = r.Dosage,
        Frequency    = r.Frequency,
        DurationDays = r.DurationDays,
        Route        = r.Route,
        Instructions = r.Instructions,
        Quantity     = r.Quantity
    };
}

// ── DTOs ──────────────────────────────────────────────────────────────────

public record PrescriptionItemRequest(
    string  MedicineName,
    string? Dosage,
    string? Frequency,
    int?    DurationDays,
    string? Route,
    string? Instructions,
    int?    Quantity);

public record CreatePrescriptionRequest(
    Guid                           PatientId,
    Guid                           DoctorId,
    Guid?                          VisitId,
    DateTime?                      ExpiresAt,
    string?                        DiagnosisSummary,
    string?                        Notes,
    List<PrescriptionItemRequest>? Items);

public record UpdatePrescriptionRequest(
    string?   DiagnosisSummary,
    string?   Notes,
    DateTime? ExpiresAt);
