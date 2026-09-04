using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ServiceCode { get; set; }
    public ServiceCategory Category { get; set; } = ServiceCategory.Consultation;

    public Invoice Invoice { get; set; } = null!;
}

public enum ServiceCategory { Consultation, Laboratory, Radiology, Procedure, Medication, Other }
