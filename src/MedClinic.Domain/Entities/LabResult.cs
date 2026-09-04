using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResult : BaseEntity
{
    public Guid LabOrderId { get; set; }
    public DateTime ResultDate { get; set; } = DateTime.UtcNow;
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public string? FileUrl { get; set; }
    public string? PerformedBy { get; set; }
    public bool IsReviewed { get; set; } = false;
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByDoctorId { get; set; }
    public bool HasAbnormalValues { get; set; } = false;

    public LabOrder LabOrder { get; set; } = null!;
    public ICollection<LabResultItem> Items { get; set; } = [];
}
