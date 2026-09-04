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
public class LabOrdersController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public LabOrdersController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ──────────────────────────────────────────────────────────────────────
    // LIST
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>List lab orders with filters</summary>
    [HttpGet]
    [HasPermission(Permissions.LabRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   patientId,
        [FromQuery] Guid?   doctorId,
        [FromQuery] string? status,
        [FromQuery] bool?   isUrgent,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.LabOrders
            .Where(o => o.ClinicId == clinicId)
            .Include(o => o.Patient)
            .Include(o => o.Doctor).ThenInclude(d => d.User)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(o => o.PatientId == patientId);
        if (doctorId.HasValue)  query = query.Where(o => o.DoctorId  == doctorId);
        if (isUrgent.HasValue)  query = query.Where(o => o.IsUrgent  == isUrgent);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status.ToString() == status);
        if (from.HasValue) query = query.Where(o => o.OrderedAt >= from);
        if (to.HasValue)   query = query.Where(o => o.OrderedAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.OrderedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.TestName,
                o.OrderedAt,
                o.CollectedAt,
                o.Status,
                o.IsUrgent,
                o.VisitId,
                Patient = new { o.Patient.Id, o.Patient.FirstName, o.Patient.LastName },
                Doctor  = new
                {
                    o.Doctor.Id,
                    Name = o.Doctor.User.FirstName + " " + o.Doctor.User.LastName
                }
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

    // ──────────────────────────────────────────────────────────────────────
    // GET BY ID
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Get lab order with results</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.LabRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var order = await _context.LabOrders
            .Where(o => o.Id == id && o.ClinicId == clinicId)
            .Include(o => o.Patient)
            .Include(o => o.Doctor).ThenInclude(d => d.User)
            .Include(o => o.Results).ThenInclude(r => r.Items)
            .FirstOrDefaultAsync(ct);

        if (order == null) return NotFound("Lab order not found.");

        return Success(new
        {
            order.Id,
            order.TestName,
            order.OrderedAt,
            order.CollectedAt,
            order.Status,
            order.IsUrgent,
            order.Notes,
            order.ClinicalInfo,
            order.VisitId,
            Patient = new { order.Patient.Id, order.Patient.FirstName, order.Patient.LastName, order.Patient.DateOfBirth },
            Doctor  = new
            {
                order.Doctor.Id,
                Name = order.Doctor.User.FirstName + " " + order.Doctor.User.LastName,
                order.Doctor.Specialty
            },
            Results = order.Results.Select(r => new
            {
                r.Id,
                r.ReportedAt,
                r.ReportedBy,
                r.Summary,
                r.IsAbnormal,
                Items = r.Items.Select(i => new
                {
                    i.Id,
                    i.TestParameter,
                    i.Value,
                    i.Unit,
                    i.ReferenceRange,
                    i.IsAbnormal,
                    i.Notes
                })
            })
        });
    }

    // ──────────────────────────────────────────────────────────────────────
    // CREATE
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Create a new lab order</summary>
    [HttpPost]
    [HasPermission(Permissions.LabCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLabOrderRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId && p.ClinicId == clinicId, ct);
        if (!patientExists) return NotFound("Patient not found.");

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId && d.ClinicId == clinicId, ct);
        if (!doctorExists) return NotFound("Doctor not found.");

        if (request.VisitId.HasValue)
        {
            var visitOk = await _context.Visits.AnyAsync(v =>
                v.Id == request.VisitId &&
                v.ClinicId == clinicId &&
                v.PatientId == request.PatientId, ct);
            if (!visitOk) return BadRequest("Visit does not match patient or clinic.");
        }

        var order = new LabOrder
        {
            ClinicId     = clinicId,
            PatientId    = request.PatientId,
            DoctorId     = request.DoctorId,
            VisitId      = request.VisitId,
            TestName     = request.TestName,
            Notes        = request.Notes,
            ClinicalInfo = request.ClinicalInfo,
            IsUrgent     = request.IsUrgent,
            Status       = LabOrderStatus.Pending,
            OrderedAt    = DateTime.UtcNow,
            CreatedBy    = CurrentUserId
        };

        _context.LabOrders.Add(order);
        await _context.SaveChangesAsync(ct);

        return Created(new { order.Id, order.TestName, order.Status, order.IsUrgent }, "Lab order created.");
    }

    // ──────────────────────────────────────────────────────────────────────
    // STATUS TRANSITIONS
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Mark sample as collected</summary>
    [HttpPost("{id:guid}/collect")]
    [HasPermission(Permissions.LabUpdate)]
    public async Task<IActionResult> MarkCollected(Guid id, CancellationToken ct)
    {
        var order = await GetOwnedOrder(id, ct);
        if (order == null) return NotFound("Lab order not found.");
        if (order.Status != LabOrderStatus.Pending)
            return BadRequest($"Order is {order.Status}, cannot collect.");

        order.Status      = LabOrderStatus.Collected;
        order.CollectedAt = DateTime.UtcNow;
        order.UpdatedBy   = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Sample marked as collected.");
    }

    /// <summary>Mark order as processing</summary>
    [HttpPost("{id:guid}/process")]
    [HasPermission(Permissions.LabUpdate)]
    public async Task<IActionResult> MarkProcessing(Guid id, CancellationToken ct)
    {
        var order = await GetOwnedOrder(id, ct);
        if (order == null) return NotFound("Lab order not found.");
        if (order.Status != LabOrderStatus.Collected)
            return BadRequest($"Order must be Collected before processing.");

        order.Status    = LabOrderStatus.Processing;
        order.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Order marked as processing.");
    }

    /// <summary>Cancel lab order</summary>
    [HttpPost("{id:guid}/cancel")]
    [HasPermission(Permissions.LabUpdate)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var order = await GetOwnedOrder(id, ct);
        if (order == null) return NotFound("Lab order not found.");
        if (order.Status == LabOrderStatus.Completed)
            return BadRequest("Cannot cancel a completed order.");

        order.Status    = LabOrderStatus.Cancelled;
        order.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Lab order cancelled.");
    }

    // ──────────────────────────────────────────────────────────────────────
    // HELPER
    // ──────────────────────────────────────────────────────────────────────

    private async Task<LabOrder?> GetOwnedOrder(Guid id, CancellationToken ct)
        => await _context.LabOrders
            .FirstOrDefaultAsync(o => o.Id == id && o.ClinicId == ClinicId, ct);
}

// ── DTOs ────────────────────────────────────────────────────────────────

public record CreateLabOrderRequest(
    Guid    PatientId,
    Guid    DoctorId,
    Guid?   VisitId,
    string  TestName,
    string? Notes,
    string? ClinicalInfo,
    bool    IsUrgent);
