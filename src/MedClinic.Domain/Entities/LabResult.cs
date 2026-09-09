using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResult : BaseEntity
{
    public Guid     LabOrderId  { get; set; }
    public Guid     OrderId     { get => LabOrderId; set => LabOrderId = value; }
    public DateTime ReportedAt  { get; set; } = DateTime.UtcNow;
    public DateTime ResultDate  { get => ReportedAt; set => ReportedAt = value; }
    public string?  ReportedBy  { get; set; }
    public string?  Summary     { get; set; }
    public bool     IsAbnormal  { get; set; } = false;
    public string   Status      { get; set; } = "Final";

    public LabOrder                  LabOrder { get; set; } = null!;
    public ICollection<LabResultItem> Items   { get; set; } = [];
}
