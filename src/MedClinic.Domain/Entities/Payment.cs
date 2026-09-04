using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // Cash, CreditCard, BankTransfer, Insurance
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public Guid RecordedByUserId { get; set; }

    public Invoice Invoice { get; set; } = null!;
}

public enum PaymentStatus { Pending, Completed, Failed, Refunded }
