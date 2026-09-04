using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResultItem : BaseEntity
{
    public Guid    LabResultId    { get; set; }
    public string  TestParameter  { get; set; } = string.Empty; // e.g. "Hemoglobin"
    public string? Value          { get; set; } // e.g. "13.5"
    public string? Unit           { get; set; } // e.g. "g/dL"
    public string? ReferenceRange { get; set; } // e.g. "12.0 - 17.5"
    public bool    IsAbnormal     { get; set; } = false;
    public string? Notes          { get; set; }

    public LabResult LabResult { get; set; } = null!;
}
