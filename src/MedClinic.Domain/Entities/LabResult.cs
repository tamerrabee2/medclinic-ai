using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResult : BaseEntity
{
    public Guid     LabOrderId  { get; set; }
    public DateTime ReportedAt  { get; set; } = DateTime.UtcNow;
    public string?  ReportedBy  { get; set; }
    public string?  Summary     { get; set; }
    public bool     IsAbnormal  { get; set; } = false;

    public LabOrder                  LabOrder { get; set; } = null!;
    public ICollection<LabResultItem> Items   { get; set; } = [];
}
