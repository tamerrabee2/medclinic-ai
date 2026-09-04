using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Discount { get; set; }
    public decimal TotalPrice => (UnitPrice * Quantity) - Discount;
}
