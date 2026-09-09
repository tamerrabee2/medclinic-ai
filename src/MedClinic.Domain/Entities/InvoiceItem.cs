using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid    InvoiceId   { get; set; }
    public string  Description { get; set; } = string.Empty;
    public string? ServiceType { get; set; } // Consultation, Lab, Radiology, Pharmacy, Procedure
    public string? ServiceCode { get; set; }
    public int     Quantity    { get; set; } = 1;
    public decimal UnitPrice   { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalPrice  { get; set; }  // Quantity * UnitPrice - DiscountAmount

    public Invoice Invoice { get; set; } = null!;
}
