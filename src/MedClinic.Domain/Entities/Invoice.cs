using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Invoice : TenantEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance => TotalAmount - PaidAmount;
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<InvoiceItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
