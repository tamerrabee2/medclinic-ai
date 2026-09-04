using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResultItem : BaseEntity
{
    public Guid LabResultId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public AbnormalFlag AbnormalFlag { get; set; } = AbnormalFlag.Normal;
    public string? Notes { get; set; }
    public int SortOrder { get; set; } = 0;

    public LabResult LabResult { get; set; } = null!;
}

public enum AbnormalFlag { Normal, Low, High, Critical, Abnormal }
