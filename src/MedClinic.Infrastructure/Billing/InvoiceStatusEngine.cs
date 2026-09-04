using MedClinic.Domain.Entities;

namespace MedClinic.Infrastructure.Billing;

/// <summary>
/// Determines the correct InvoiceStatus based on amounts and due date.
/// Centralised so every payment/refund path uses the same rules.
/// </summary>
public class InvoiceStatusEngine
{
    /// <summary>
    /// Recalculate status after any payment change.
    /// Never downgrades a Cancelled or Draft invoice.
    /// </summary>
    public InvoiceStatus Calculate(Invoice invoice)
    {
        // Immutable statuses — never auto-change these
        if (invoice.Status is InvoiceStatus.Cancelled or
                              InvoiceStatus.Draft)
            return invoice.Status;

        var outstanding = invoice.TotalAmount - invoice.PaidAmount;

        // Fully paid or overpaid (refund edge case)
        if (outstanding <= 0)
            return InvoiceStatus.Paid;

        // Partial payment received
        if (invoice.PaidAmount > 0)
            return InvoiceStatus.PartiallyPaid;

        // No payment yet — check if overdue
        if (invoice.DueDate.HasValue && invoice.DueDate < DateTime.UtcNow)
            return InvoiceStatus.Overdue;

        // Previously Sent (or Overdue downgraded by refund)
        return InvoiceStatus.Sent;
    }

    /// <summary>
    /// Returns true if the invoice can accept more payments.
    /// </summary>
    public bool CanAcceptPayment(Invoice invoice)
        => invoice.Status is not (InvoiceStatus.Paid
            or InvoiceStatus.Cancelled
            or InvoiceStatus.Draft
            or InvoiceStatus.Refunded);

    /// <summary>
    /// Returns outstanding balance.
    /// </summary>
    public decimal OutstandingBalance(Invoice invoice)
        => Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
}
