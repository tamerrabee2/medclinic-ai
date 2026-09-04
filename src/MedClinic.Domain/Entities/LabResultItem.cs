using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResultItem : BaseEntity
{
    public Guid LabResultId { get; set; }
    public LabResult LabResult { get; set; } = null!;
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public bool IsAbnormal { get; set; } = false;
    public string? Flag { get; set; }
}
