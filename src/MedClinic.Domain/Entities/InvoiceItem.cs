using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid    InvoiceId   { get; set; }
    public string  Description { get; set; } = string.Empty;
    public string? ServiceType { get; set; } // Consultation, Lab, Radiology, Pharmacy, Procedure
    public int     Quantity    { get; set; } = 1;
    public decimal UnitPrice   { get; set; }
    public decimal TotalPrice  { get; set; }  // Quantity * UnitPrice

    public Invoice Invoice { get; set; } = null!;
}
