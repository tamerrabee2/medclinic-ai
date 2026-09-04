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
[Route("api/v1/lab-orders/{orderId:guid}/results")]
public class LabResultsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public LabResultsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ──────────────────────────────────────────────────────────────────────
    // ENTER RESULTS (LabTechnician)
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Enter lab results for an order (marks it Completed)</summary>
    [HttpPost]
    [HasPermission(Permissions.LabEnterResults)]
    public async Task<IActionResult> EnterResults(
        Guid orderId,
        [FromBody] EnterLabResultsRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var order = await _context.LabOrders
            .Include(o => o.Results)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.ClinicId == clinicId, ct);

        if (order == null) return NotFound("Lab order not found.");
        if (order.Status == LabOrderStatus.Cancelled)
            return BadRequest("Cannot enter results for a cancelled order.");
        if (order.Status == LabOrderStatus.Completed)
            return BadRequest("Results already entered. Use update endpoint.");

        var result = new LabResult
        {
            LabOrderId   = orderId,
            ReportedAt   = DateTime.UtcNow,
            ReportedBy   = request.ReportedBy,
            Summary      = request.Summary,
            IsAbnormal   = request.Items.Any(i => i.IsAbnormal),
            CreatedBy    = CurrentUserId
        };

        if (request.Items.Count > 0)
            result.Items = request.Items.Select(i => new LabResultItem
            {
                TestParameter  = i.TestParameter,
                Value          = i.Value,
                Unit           = i.Unit,
                ReferenceRange = i.ReferenceRange,
                IsAbnormal     = i.IsAbnormal,
                Notes          = i.Notes
            }).ToList();

        _context.LabResults.Add(result);

        // Auto-complete order
        order.Status    = LabOrderStatus.Completed;
        order.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);

        return Created(new
        {
            result.Id,
            result.ReportedAt,
            result.IsAbnormal,
            ItemCount = result.Items.Count
        }, "Lab results entered.");
    }

    // ──────────────────────────────────────────────────────────────────────
    // GET RESULTS
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Get all results for a lab order</summary>
    [HttpGet]
    [HasPermission(Permissions.LabRead)]
    public async Task<IActionResult> GetResults(Guid orderId, CancellationToken ct)
    {
        var clinicId = ClinicId;

        var orderExists = await _context.LabOrders
            .AnyAsync(o => o.Id == orderId && o.ClinicId == clinicId, ct);
        if (!orderExists) return NotFound("Lab order not found.");

        var results = await _context.LabResults
            .Where(r => r.LabOrderId == orderId)
            .Include(r => r.Items)
            .OrderByDescending(r => r.ReportedAt)
            .Select(r => new
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
            .ToListAsync(ct);

        return Success(results);
    }

    // ──────────────────────────────────────────────────────────────────────
    // UPDATE RESULT
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Update/amend an existing lab result</summary>
    [HttpPut("{resultId:guid}")]
    [HasPermission(Permissions.LabEnterResults)]
    public async Task<IActionResult> UpdateResult(
        Guid orderId,
        Guid resultId,
        [FromBody] UpdateLabResultRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var orderExists = await _context.LabOrders
            .AnyAsync(o => o.Id == orderId && o.ClinicId == clinicId, ct);
        if (!orderExists) return NotFound("Lab order not found.");

        var result = await _context.LabResults
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == resultId && r.LabOrderId == orderId, ct);

        if (result == null) return NotFound("Lab result not found.");

        if (request.Summary    != null) result.Summary    = request.Summary;
        if (request.ReportedBy != null) result.ReportedBy = request.ReportedBy;

        // Replace items if provided
        if (request.Items?.Count > 0)
        {
            _context.LabResultItems.RemoveRange(result.Items);
            result.Items = request.Items.Select(i => new LabResultItem
            {
                TestParameter  = i.TestParameter,
                Value          = i.Value,
                Unit           = i.Unit,
                ReferenceRange = i.ReferenceRange,
                IsAbnormal     = i.IsAbnormal,
                Notes          = i.Notes
            }).ToList();
            result.IsAbnormal = result.Items.Any(i => i.IsAbnormal);
        }

        result.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Result updated.");
    }

    // ──────────────────────────────────────────────────────────────────────
    // PATIENT LAB HISTORY
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Get all lab orders + results for a patient</summary>
    [HttpGet("/api/v1/patients/{patientId:guid}/lab-history")]
    [HasPermission(Permissions.LabRead)]
    public async Task<IActionResult> GetPatientLabHistory(
        Guid patientId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 50);
        var clinicId = ClinicId;

        var total = await _context.LabOrders
            .CountAsync(o => o.PatientId == patientId && o.ClinicId == clinicId, ct);

        var orders = await _context.LabOrders
            .Where(o => o.PatientId == patientId && o.ClinicId == clinicId)
            .Include(o => o.Doctor).ThenInclude(d => d.User)
            .Include(o => o.Results)
            .OrderByDescending(o => o.OrderedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.TestName,
                o.OrderedAt,
                o.Status,
                o.IsUrgent,
                Doctor = new
                {
                    o.Doctor.Id,
                    Name = o.Doctor.User.FirstName + " " + o.Doctor.User.LastName
                },
                HasResults  = o.Results.Any(),
                IsAbnormal  = o.Results.Any(r => r.IsAbnormal)
            })
            .ToListAsync(ct);

        return Success(new
        {
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            Items      = orders
        });
    }
}

// ── DTOs ────────────────────────────────────────────────────────────────

public record LabResultItemRequest(
    string  TestParameter,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    bool    IsAbnormal,
    string? Notes);

public record EnterLabResultsRequest(
    string                   ReportedBy,
    string?                  Summary,
    List<LabResultItemRequest> Items);

public record UpdateLabResultRequest(
    string?                        Summary,
    string?                        ReportedBy,
    List<LabResultItemRequest>?    Items);
