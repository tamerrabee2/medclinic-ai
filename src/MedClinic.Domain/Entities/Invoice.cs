using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Invoice : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? VisitId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal OutstandingBalance => TotalAmount - PaidAmount;
    public string Currency { get; set; } = "USD";
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Notes { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<InvoiceItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}

public enum InvoiceStatus { Draft, Sent, Paid, PartiallyPaid, Overdue, Cancelled, Refunded }
