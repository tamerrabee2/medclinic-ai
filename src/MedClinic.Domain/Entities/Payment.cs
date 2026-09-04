using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Payment : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Clinic Clinic { get; set; } = null!;
}
