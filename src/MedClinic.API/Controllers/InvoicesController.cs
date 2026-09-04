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
public class InvoicesController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public InvoicesController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ─────────────────────────────────────────────────────────────────────
    // LIST
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>List invoices with filters and pagination</summary>
    [HttpGet]
    [HasPermission(Permissions.BillingRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   patientId,
        [FromQuery] string? status,
        [FromQuery] bool?   overdue,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Invoices
            .Where(i => i.ClinicId == clinicId)
            .Include(i => i.Patient)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(i => i.PatientId == patientId);
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
            query = query.Where(i => i.Status == parsedStatus);
        if (overdue == true)
            query = query.Where(i => i.DueDate < DateTime.UtcNow
                && i.Status != InvoiceStatus.Paid
                && i.Status != InvoiceStatus.Cancelled);
        if (from.HasValue) query = query.Where(i => i.IssuedAt >= from);
        if (to.HasValue)   query = query.Where(i => i.IssuedAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.IssuedAt,
                i.DueDate,
                i.TotalAmount,
                i.PaidAmount,
                i.DiscountAmount,
                i.TaxAmount,
                i.Currency,
                i.Status,
                OutstandingBalance = i.TotalAmount - i.PaidAmount,
                Patient = new { i.Patient.Id, i.Patient.FirstName, i.Patient.LastName }
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

    /// <summary>Get full invoice detail with items and payments</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.BillingRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var invoice = await _context.Invoices
            .Where(i => i.Id == id && i.ClinicId == clinicId)
            .Include(i => i.Patient)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(ct);

        if (invoice == null) return NotFound("Invoice not found.");

        return Success(new
        {
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.IssuedAt,
            invoice.DueDate,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.DiscountAmount,
            invoice.TaxAmount,
            invoice.Currency,
            invoice.Status,
            invoice.Notes,
            invoice.VisitId,
            OutstandingBalance = invoice.TotalAmount - invoice.PaidAmount,
            Patient = new
            {
                invoice.Patient.Id,
                invoice.Patient.FirstName,
                invoice.Patient.LastName,
                invoice.Patient.Phone,
                invoice.Patient.Email
            },
            Items = invoice.Items.Select(item => new
            {
                item.Id,
                item.Description,
                item.ServiceType,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice
            }),
            Payments = invoice.Payments.Select(p => new
            {
                p.Id,
                p.Amount,
                p.Method,
                p.PaidAt,
                p.ReferenceNumber,
                p.Status,
                p.Notes
            })
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Create a new invoice (Draft status)</summary>
    [HttpPost]
    [HasPermission(Permissions.BillingCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId && p.ClinicId == clinicId, ct);
        if (!patientExists) return NotFound("Patient not found.");

        if (request.VisitId.HasValue)
        {
            var visitOk = await _context.Visits.AnyAsync(v =>
                v.Id == request.VisitId &&
                v.ClinicId == clinicId &&
                v.PatientId == request.PatientId, ct);
            if (!visitOk) return BadRequest("Visit does not match patient or clinic.");
        }

        // Generate unique invoice number: INV-{year}-{seq}
        var year     = DateTime.UtcNow.Year;
        var count    = await _context.Invoices.CountAsync(i => i.ClinicId == clinicId, ct);
        var invoiceNo = $"INV-{year}-{(count + 1):D5}";

        var invoice = new Invoice
        {
            ClinicId       = clinicId,
            PatientId      = request.PatientId,
            DoctorId       = request.DoctorId,
            VisitId        = request.VisitId,
            InvoiceNumber  = invoiceNo,
            IssuedAt       = DateTime.UtcNow,
            DueDate        = request.DueDate,
            Currency       = request.Currency ?? "USD",
            DiscountAmount = request.DiscountAmount,
            TaxAmount      = request.TaxAmount,
            Notes          = request.Notes,
            Status         = InvoiceStatus.Draft,
            CreatedBy      = CurrentUserId
        };

        if (request.Items?.Count > 0)
        {
            invoice.Items = request.Items.Select(MapItem).ToList();
            RecalculateTotal(invoice);
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(ct);

        return Created(new
        {
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.TotalAmount,
            invoice.Status
        }, "Invoice created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Update invoice header (Draft only)</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateInvoiceRequest request,
        CancellationToken ct)
    {
        var invoice = await GetOwnedInvoice(id, ct);
        if (invoice == null) return NotFound("Invoice not found.");
        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest("Only Draft invoices can be edited.");

        if (request.DueDate.HasValue)          invoice.DueDate        = request.DueDate;
        if (request.DiscountAmount.HasValue)   invoice.DiscountAmount = request.DiscountAmount.Value;
        if (request.TaxAmount.HasValue)        invoice.TaxAmount      = request.TaxAmount.Value;
        if (request.Notes != null)             invoice.Notes          = request.Notes;
        if (request.Currency != null)          invoice.Currency       = request.Currency;

        RecalculateTotal(invoice);
        invoice.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Invoice updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // SEND (Draft → Sent)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Send invoice to patient (Draft → Sent)</summary>
    [HttpPost("{id:guid}/send")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        var invoice = await GetOwnedInvoice(id, ct);
        if (invoice == null) return NotFound("Invoice not found.");
        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest("Only Draft invoices can be sent.");
        if (!invoice.Items.Any())
            return BadRequest("Cannot send an empty invoice.");

        invoice.Status    = InvoiceStatus.Sent;
        invoice.UpdatedBy = CurrentUserId;
        // TODO: Send email notification (Phase 7 — Notifications)

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Invoice sent.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // CANCEL
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Cancel an invoice (must not be fully paid)</summary>
    [HttpPost("{id:guid}/cancel")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var invoice = await GetOwnedInvoice(id, ct);
        if (invoice == null) return NotFound("Invoice not found.");
        if (invoice.Status == InvoiceStatus.Paid)
            return BadRequest("Cannot cancel a fully paid invoice.");
        if (invoice.Status == InvoiceStatus.Cancelled)
            return BadRequest("Invoice already cancelled.");

        invoice.Status    = InvoiceStatus.Cancelled;
        invoice.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Invoice cancelled.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ITEMS — ADD
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Add a service/item to invoice</summary>
    [HttpPost("{id:guid}/items")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] InvoiceItemRequest request,
        CancellationToken ct)
    {
        var invoice = await GetOwnedInvoice(id, ct);
        if (invoice == null) return NotFound("Invoice not found.");
        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest("Can only add items to Draft invoices.");

        var item = MapItem(request);
        item.InvoiceId = invoice.Id;
        _context.InvoiceItems.Add(item);

        // Reload items for recalculation
        await _context.Entry(invoice)
            .Collection(i => i.Items)
            .LoadAsync(ct);
        invoice.Items.Add(item);
        RecalculateTotal(invoice);
        invoice.UpdatedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Created(new { item.Id, item.Description, item.TotalPrice }, "Item added.");
    }

    /// <summary>Update an invoice item</summary>
    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> UpdateItem(
        Guid id, Guid itemId,
        [FromBody] InvoiceItemRequest request,
        CancellationToken ct)
    {
        var invoice = await GetOwnedInvoice(id, ct);
        if (invoice == null) return NotFound("Invoice not found.");
        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest("Can only edit items on Draft invoices.");

        var item = await _context.InvoiceItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.InvoiceId == id, ct);
        if (item == null) return NotFound("Item not found.");

        item.Description = request.Description;
        item.ServiceType = request.ServiceType;
        item.Quantity    = request.Quantity;
        item.UnitPrice   = request.UnitPrice;
        item.TotalPrice  = request.Quantity * request.UnitPrice;

        await _context.Entry(invoice).Collection(i => i.Items).LoadAsync(ct);
        RecalculateTotal(invoice);
        invoice.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Item updated.");
    }

    /// <summary>Delete an invoice item</summary>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> DeleteItem(
        Guid id, Guid itemId,
        CancellationToken ct)
    {
        var invoice = await GetOwnedInvoice(id, ct);
        if (invoice == null) return NotFound("Invoice not found.");
        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest("Can only remove items from Draft invoices.");

        var item = await _context.InvoiceItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.InvoiceId == id, ct);
        if (item == null) return NotFound("Item not found.");

        _context.InvoiceItems.Remove(item);
        await _context.Entry(invoice).Collection(i => i.Items).LoadAsync(ct);
        invoice.Items.Remove(item);
        RecalculateTotal(invoice);
        invoice.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Item removed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // PATIENT BILLING HISTORY
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Get billing history for a patient</summary>
    [HttpGet("/api/v1/patients/{patientId:guid}/billing-history")]
    [HasPermission(Permissions.BillingRead)]
    public async Task<IActionResult> GetPatientBillingHistory(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 50);
        var clinicId = ClinicId;

        var total = await _context.Invoices
            .CountAsync(i => i.PatientId == patientId && i.ClinicId == clinicId, ct);

        var invoices = await _context.Invoices
            .Where(i => i.PatientId == patientId && i.ClinicId == clinicId)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.IssuedAt,
                i.DueDate,
                i.TotalAmount,
                i.PaidAmount,
                i.Currency,
                i.Status,
                OutstandingBalance = i.TotalAmount - i.PaidAmount,
                ItemCount    = i.Items.Count,
                PaymentCount = i.Payments.Count
            })
            .ToListAsync(ct);

        return Success(new
        {
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            Items      = invoices
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    private async Task<Invoice?> GetOwnedInvoice(Guid id, CancellationToken ct)
        => await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id && i.ClinicId == ClinicId, ct);

    private static InvoiceItem MapItem(InvoiceItemRequest r) => new()
    {
        Description = r.Description,
        ServiceType = r.ServiceType,
        Quantity    = r.Quantity,
        UnitPrice   = r.UnitPrice,
        TotalPrice  = r.Quantity * r.UnitPrice
    };

    private static void RecalculateTotal(Invoice invoice)
    {
        var subTotal = invoice.Items.Sum(i => i.TotalPrice);
        invoice.TotalAmount = subTotal - invoice.DiscountAmount + invoice.TaxAmount;
        if (invoice.TotalAmount < 0) invoice.TotalAmount = 0;
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────

public record InvoiceItemRequest(
    string  Description,
    string? ServiceType,   // Consultation, Lab, Radiology, Pharmacy, Procedure
    int     Quantity,
    decimal UnitPrice);

public record CreateInvoiceRequest(
    Guid                        PatientId,
    Guid?                       DoctorId,
    Guid?                       VisitId,
    DateTime?                   DueDate,
    string?                     Currency,
    decimal                     DiscountAmount,
    decimal                     TaxAmount,
    string?                     Notes,
    List<InvoiceItemRequest>?   Items);

public record UpdateInvoiceRequest(
    DateTime? DueDate,
    decimal?  DiscountAmount,
    decimal?  TaxAmount,
    string?   Notes,
    string?   Currency);
