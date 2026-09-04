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
[Route("api/v1/billing/reports")]
public class BillingReportsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;

    public BillingReportsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ───────────────────────────────────────────────────────────────────
    // SUMMARY (KPIs)
    // ───────────────────────────────────────────────────────────────────

    /// <summary>High-level billing KPIs for a date range</summary>
    [HttpGet("summary")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-1);
        var end      = to   ?? DateTime.UtcNow;

        // Invoice stats
        var invoices = await _context.Invoices
            .Where(i => i.ClinicId == clinicId &&
                        i.IssuedAt >= start &&
                        i.IssuedAt <= end)
            .GroupBy(i => 1)
            .Select(g => new
            {
                Total         = g.Count(),
                TotalBilled   = g.Sum(i => i.TotalAmount),
                TotalPaid     = g.Sum(i => i.PaidAmount),
                TotalDiscount = g.Sum(i => i.DiscountAmount),
                TotalTax      = g.Sum(i => i.TaxAmount),
                Draft         = g.Count(i => i.Status == InvoiceStatus.Draft),
                Sent          = g.Count(i => i.Status == InvoiceStatus.Sent),
                Paid          = g.Count(i => i.Status == InvoiceStatus.Paid),
                PartiallyPaid = g.Count(i => i.Status == InvoiceStatus.PartiallyPaid),
                Overdue       = g.Count(i => i.Status == InvoiceStatus.Overdue),
                Cancelled     = g.Count(i => i.Status == InvoiceStatus.Cancelled)
            })
            .FirstOrDefaultAsync(ct);

        // Payment stats
        var payments = await _context.Payments
            .Where(p => p.Invoice.ClinicId == clinicId &&
                        p.PaidAt >= start &&
                        p.PaidAt <= end &&
                        p.Status == PaymentStatus.Completed)
            .GroupBy(p => 1)
            .Select(g => new
            {
                TotalPayments = g.Count(),
                TotalCollected = g.Sum(p => p.Amount)
            })
            .FirstOrDefaultAsync(ct);

        // Outstanding across all invoices (not just date range)
        var outstanding = await _context.Invoices
            .Where(i => i.ClinicId == clinicId &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.Status != InvoiceStatus.Draft)
            .SumAsync(i => i.TotalAmount - i.PaidAmount, ct);

        return Success(new
        {
            Period = new { From = start, To = end },
            Invoices = invoices ?? new
            {
                Total = 0, TotalBilled = 0m, TotalPaid = 0m,
                TotalDiscount = 0m, TotalTax = 0m,
                Draft = 0, Sent = 0, Paid = 0,
                PartiallyPaid = 0, Overdue = 0, Cancelled = 0
            },
            Payments = payments ?? new { TotalPayments = 0, TotalCollected = 0m },
            TotalOutstandingBalance = Math.Max(0, outstanding),
            CollectionRate = invoices?.TotalBilled > 0
                ? Math.Round((decimal)(payments?.TotalCollected ?? 0) / invoices.TotalBilled * 100, 2)
                : 0m
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // REVENUE BY PERIOD (daily / monthly)
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Revenue grouped by day or month</summary>
    [HttpGet("revenue")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string    groupBy  = "day",   // day | month
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-3);
        var end      = to   ?? DateTime.UtcNow;

        var payments = await _context.Payments
            .Where(p =>
                p.Invoice.ClinicId == clinicId &&
                p.PaidAt >= start &&
                p.PaidAt <= end &&
                p.Status == PaymentStatus.Completed &&
                p.Amount > 0)
            .Select(p => new { p.PaidAt, p.Amount })
            .ToListAsync(ct);

        var grouped = groupBy.ToLower() == "month"
            ? payments
                .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Period   = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue  = g.Sum(p => p.Amount),
                    Payments = g.Count()
                })
            : payments
                .GroupBy(p => p.PaidAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Period   = g.Key.ToString("yyyy-MM-dd"),
                    Revenue  = g.Sum(p => p.Amount),
                    Payments = g.Count()
                });

        return Success(new
        {
            GroupBy = groupBy,
            From    = start,
            To      = end,
            Data    = grouped
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // REVENUE BY SERVICE TYPE
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Revenue breakdown by service type (Consultation, Lab, Radiology...)</summary>
    [HttpGet("by-service")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetByServiceType(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-1);
        var end      = to   ?? DateTime.UtcNow;

        var breakdown = await _context.InvoiceItems
            .Where(item =>
                item.Invoice.ClinicId == clinicId &&
                item.Invoice.IssuedAt >= start &&
                item.Invoice.IssuedAt <= end &&
                item.Invoice.Status   != InvoiceStatus.Cancelled)
            .GroupBy(item => item.ServiceType ?? "Other")
            .Select(g => new
            {
                ServiceType  = g.Key,
                TotalRevenue = g.Sum(i => i.TotalPrice),
                ItemCount    = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync(ct);

        var grandTotal = breakdown.Sum(b => b.TotalRevenue);

        return Success(new
        {
            From    = start,
            To      = end,
            Total   = grandTotal,
            Breakdown = breakdown.Select(b => new
            {
                b.ServiceType,
                b.TotalRevenue,
                b.ItemCount,
                Percentage = grandTotal > 0
                    ? Math.Round(b.TotalRevenue / grandTotal * 100, 2)
                    : 0m
            })
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // PAYMENT METHODS BREAKDOWN
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Breakdown of collections by payment method</summary>
    [HttpGet("by-payment-method")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetByPaymentMethod(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-1);
        var end      = to   ?? DateTime.UtcNow;

        var breakdown = await _context.Payments
            .Where(p =>
                p.Invoice.ClinicId == clinicId &&
                p.PaidAt  >= start &&
                p.PaidAt  <= end   &&
                p.Status  == PaymentStatus.Completed &&
                p.Amount  > 0)
            .GroupBy(p => p.Method)
            .Select(g => new
            {
                Method   = g.Key,
                Total    = g.Sum(p => p.Amount),
                Count    = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync(ct);

        var grandTotal = breakdown.Sum(b => b.Total);

        return Success(new
        {
            From  = start,
            To    = end,
            Total = grandTotal,
            Breakdown = breakdown.Select(b => new
            {
                b.Method,
                b.Total,
                b.Count,
                Percentage = grandTotal > 0
                    ? Math.Round(b.Total / grandTotal * 100, 2)
                    : 0m
            })
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // TOP PATIENTS BY REVENUE
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Top N patients by total billed amount</summary>
    [HttpGet("top-patients")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetTopPatients(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int       top  = 10,
        CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 50);
        var clinicId = ClinicId;
        var start    = from ?? DateTime.UtcNow.AddMonths(-1);
        var end      = to   ?? DateTime.UtcNow;

        var result = await _context.Invoices
            .Where(i =>
                i.ClinicId == clinicId &&
                i.IssuedAt >= start    &&
                i.IssuedAt <= end      &&
                i.Status   != InvoiceStatus.Cancelled)
            .GroupBy(i => new
            {
                i.PatientId,
                i.Patient.FirstName,
                i.Patient.LastName
            })
            .Select(g => new
            {
                PatientId    = g.Key.PatientId,
                PatientName  = g.Key.FirstName + " " + g.Key.LastName,
                TotalBilled  = g.Sum(i => i.TotalAmount),
                TotalPaid    = g.Sum(i => i.PaidAmount),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(x => x.TotalBilled)
            .Take(top)
            .ToListAsync(ct);

        return Success(new { From = start, To = end, Top = top, Patients = result });
    }

    // ───────────────────────────────────────────────────────────────────
    // TODAY’S CASH FLOW
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Today’s cash flow — invoiced, collected, refunded</summary>
    [HttpGet("today")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetToday(CancellationToken ct = default)
    {
        var clinicId  = ClinicId;
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1);

        var invoiced = await _context.Invoices
            .Where(i => i.ClinicId == clinicId &&
                        i.IssuedAt >= todayStart &&
                        i.IssuedAt <  todayEnd   &&
                        i.Status   != InvoiceStatus.Cancelled)
            .SumAsync(i => i.TotalAmount, ct);

        var collected = await _context.Payments
            .Where(p => p.Invoice.ClinicId == clinicId &&
                        p.PaidAt  >= todayStart &&
                        p.PaidAt  <  todayEnd   &&
                        p.Status  == PaymentStatus.Completed &&
                        p.Amount  > 0)
            .SumAsync(p => p.Amount, ct);

        var refunded = await _context.Payments
            .Where(p => p.Invoice.ClinicId == clinicId &&
                        p.PaidAt  >= todayStart &&
                        p.PaidAt  <  todayEnd   &&
                        p.Status  == PaymentStatus.Refunded)
            .SumAsync(p => p.Amount, ct); // negative amounts

        var newInvoices  = await _context.Invoices
            .CountAsync(i => i.ClinicId == clinicId &&
                             i.IssuedAt >= todayStart &&
                             i.IssuedAt < todayEnd, ct);

        var newPayments  = await _context.Payments
            .CountAsync(p => p.Invoice.ClinicId == clinicId &&
                             p.PaidAt >= todayStart &&
                             p.PaidAt <  todayEnd   &&
                             p.Amount > 0, ct);

        return Success(new
        {
            Date          = todayStart.ToString("yyyy-MM-dd"),
            TotalInvoiced = invoiced,
            TotalCollected = collected,
            TotalRefunded  = Math.Abs(refunded),
            NetRevenue     = collected + refunded,  // refunded is negative
            NewInvoices    = newInvoices,
            NewPayments    = newPayments
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // OUTSTANDING AGING REPORT
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Aging of outstanding balances: 0-30, 31-60, 61-90, 90+ days</summary>
    [HttpGet("aging")]
    [HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetAging(CancellationToken ct = default)
    {
        var clinicId = ClinicId;
        var now      = DateTime.UtcNow;

        var unpaid = await _context.Invoices
            .Where(i =>
                i.ClinicId == clinicId &&
                i.Status != InvoiceStatus.Cancelled &&
                i.Status != InvoiceStatus.Draft      &&
                i.Status != InvoiceStatus.Paid       &&
                i.TotalAmount > i.PaidAmount)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.IssuedAt,
                i.DueDate,
                Balance = i.TotalAmount - i.PaidAmount,
                PatientName = i.Patient.FirstName + " " + i.Patient.LastName
            })
            .ToListAsync(ct);

        var aged = unpaid.Select(i =>
        {
            var refDate = i.DueDate ?? i.IssuedAt;
            var days    = (now - refDate).Days;
            var bucket  = days switch
            {
                <= 30  => "0-30",
                <= 60  => "31-60",
                <= 90  => "61-90",
                _      => "90+"
            };
            return new { i.Id, i.InvoiceNumber, i.PatientName, i.Balance, DaysOverdue = days, Bucket = bucket };
        });

        var buckets = aged
            .GroupBy(x => x.Bucket)
            .Select(g => new
            {
                Bucket         = g.Key,
                TotalBalance   = g.Sum(x => x.Balance),
                InvoiceCount   = g.Count()
            })
            .OrderBy(b => b.Bucket);

        return Success(new
        {
            AsOf    = now,
            Buckets = buckets,
            Details = aged.OrderByDescending(x => x.DaysOverdue).Take(50)
        });
    }
}
