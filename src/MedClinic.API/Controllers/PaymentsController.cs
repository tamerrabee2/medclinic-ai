using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Billing;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class PaymentsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;
    private readonly InvoiceStatusEngine  _statusEngine;

    public PaymentsController(
        ApplicationDbContext context,
        ITenantContext tenant,
        InvoiceStatusEngine statusEngine)
    {
        _context      = context;
        _tenant       = tenant;
        _statusEngine = statusEngine;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ───────────────────────────────────────────────────────────────────
    // LIST PAYMENTS
    // ───────────────────────────────────────────────────────────────────

    /// <summary>List payments for a clinic with filters</summary>
    [HttpGet]
    [HasPermission(Permissions.BillingRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   patientId,
        [FromQuery] Guid?   invoiceId,
        [FromQuery] string? method,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
            .Where(p => p.Invoice.ClinicId == clinicId)
            .AsQueryable();

        if (invoiceId.HasValue)  query = query.Where(p => p.InvoiceId == invoiceId);
        if (patientId.HasValue)  query = query.Where(p => p.Invoice.PatientId == patientId);
        if (!string.IsNullOrWhiteSpace(method))  query = query.Where(p => p.Method == method);
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PaymentStatus>(status, true, out var ps))
            query = query.Where(p => p.Status == ps);
        if (from.HasValue) query = query.Where(p => p.PaidAt >= from);
        if (to.HasValue)   query = query.Where(p => p.PaidAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.PaidAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Amount,
                p.Method,
                p.PaidAt,
                p.ReferenceNumber,
                p.Status,
                p.Notes,
                Invoice = new
                {
                    p.Invoice.Id,
                    p.Invoice.InvoiceNumber,
                    p.Invoice.TotalAmount,
                    p.Invoice.Currency
                },
                Patient = new
                {
                    p.Invoice.Patient.Id,
                    p.Invoice.Patient.FirstName,
                    p.Invoice.Patient.LastName
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

    // ───────────────────────────────────────────────────────────────────
    // RECORD PAYMENT
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Record a payment against an invoice</summary>
    [HttpPost]
    [HasPermission(Permissions.BillingCreate)]
    public async Task<IActionResult> RecordPayment(
        [FromBody] RecordPaymentRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i =>
                i.Id == request.InvoiceId &&
                i.ClinicId == clinicId, ct);

        if (invoice == null)
            return NotFound("Invoice not found.");
        if (invoice.Status == InvoiceStatus.Cancelled)
            return BadRequest("Cannot pay a cancelled invoice.");
        if (invoice.Status == InvoiceStatus.Paid)
            return BadRequest("Invoice is already fully paid.");
        if (invoice.Status == InvoiceStatus.Draft)
            return BadRequest("Cannot pay a Draft invoice. Send it first.");

        // Validate payment amount
        var outstanding = invoice.TotalAmount - invoice.PaidAmount;
        if (request.Amount <= 0)
            return BadRequest("Payment amount must be greater than zero.");
        if (request.Amount > outstanding)
            return BadRequest($"Payment ({request.Amount:F2}) exceeds outstanding balance ({outstanding:F2}).");

        var payment = new Payment
        {
            InvoiceId       = invoice.Id,
            Amount          = request.Amount,
            Method          = request.Method,
            PaidAt          = request.PaidAt ?? DateTime.UtcNow,
            ReferenceNumber = request.ReferenceNumber,
            Notes           = request.Notes,
            Status          = PaymentStatus.Completed,
            RecordedByUserId = CurrentUserId,
            CreatedBy       = CurrentUserId
        };

        _context.Payments.Add(payment);

        // Update invoice PaidAmount and recalculate status
        invoice.PaidAmount += request.Amount;
        invoice.Status      = _statusEngine.Calculate(invoice);
        invoice.UpdatedBy   = CurrentUserId;

        await _context.SaveChangesAsync(ct);

        return Created(new
        {
            payment.Id,
            payment.Amount,
            payment.Method,
            payment.PaidAt,
            InvoiceStatus    = invoice.Status.ToString(),
            OutstandingBalance = invoice.TotalAmount - invoice.PaidAmount
        }, "Payment recorded.");
    }

    // ───────────────────────────────────────────────────────────────────
    // REFUND
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Refund a specific payment (partial or full)</summary>
    [HttpPost("{paymentId:guid}/refund")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> Refund(
        Guid paymentId,
        [FromBody] RefundPaymentRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var payment = await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Payments)
            .FirstOrDefaultAsync(p =>
                p.Id == paymentId &&
                p.Invoice.ClinicId == clinicId, ct);

        if (payment == null)
            return NotFound("Payment not found.");
        if (payment.Status == PaymentStatus.Refunded)
            return BadRequest("Payment already refunded.");
        if (payment.Status != PaymentStatus.Completed)
            return BadRequest("Only Completed payments can be refunded.");

        var refundAmount = request.Amount ?? payment.Amount;
        if (refundAmount <= 0 || refundAmount > payment.Amount)
            return BadRequest($"Refund amount must be between 0 and {payment.Amount:F2}.");

        // Create refund record (negative payment)
        var refundPayment = new Payment
        {
            InvoiceId        = payment.InvoiceId,
            Amount           = -refundAmount,          // negative
            Method           = payment.Method,
            PaidAt           = DateTime.UtcNow,
            ReferenceNumber  = $"REFUND-{payment.ReferenceNumber ?? payment.Id.ToString()[..8]}",
            Notes            = request.Reason ?? "Refund",
            Status           = PaymentStatus.Refunded,
            RecordedByUserId = CurrentUserId,
            CreatedBy        = CurrentUserId
        };

        // Update original payment status
        payment.Status = refundAmount == payment.Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.Completed;          // partial refund

        // Recalculate invoice PaidAmount and status
        var invoice = payment.Invoice;
        invoice.PaidAmount -= refundAmount;
        if (invoice.PaidAmount < 0) invoice.PaidAmount = 0;
        invoice.Status    = _statusEngine.Calculate(invoice);
        invoice.UpdatedBy = CurrentUserId;

        _context.Payments.Add(refundPayment);
        await _context.SaveChangesAsync(ct);

        return Success(new
        {
            RefundPaymentId    = refundPayment.Id,
            RefundAmount       = refundAmount,
            InvoiceStatus      = invoice.Status.ToString(),
            OutstandingBalance = invoice.TotalAmount - invoice.PaidAmount
        }, "Refund processed.");
    }

    // ───────────────────────────────────────────────────────────────────
    // MARK OVERDUE (background-friendly, callable manually)
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Mark all eligible sent/partial invoices as Overdue (Admin only)</summary>
    [HttpPost("/api/v1/billing/mark-overdue")]
    [HasPermission(Permissions.BillingUpdate)]
    public async Task<IActionResult> MarkOverdue(CancellationToken ct)
    {
        var clinicId = ClinicId;
        var now      = DateTime.UtcNow;

        var overdueInvoices = await _context.Invoices
            .Where(i =>
                i.ClinicId == clinicId &&
                i.DueDate < now &&
                (i.Status == InvoiceStatus.Sent ||
                 i.Status == InvoiceStatus.PartiallyPaid))
            .ToListAsync(ct);

        foreach (var inv in overdueInvoices)
        {
            inv.Status    = InvoiceStatus.Overdue;
            inv.UpdatedBy = CurrentUserId;
        }

        await _context.SaveChangesAsync(ct);
        return Success(new { MarkedOverdue = overdueInvoices.Count }, $"{overdueInvoices.Count} invoice(s) marked overdue.");
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────

public record RecordPaymentRequest(
    Guid      InvoiceId,
    decimal   Amount,
    string    Method,         // Cash, CreditCard, BankTransfer, Insurance, Cheque
    DateTime? PaidAt,
    string?   ReferenceNumber,
    string?   Notes);

public record RefundPaymentRequest(
    decimal? Amount,          // null = full refund
    string?  Reason);
