using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabOrder : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? VisitId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CollectedAt { get; set; }
    public string? Notes { get; set; }
    public string? ClinicalInfo { get; set; }
    public LabOrderStatus Status { get; set; } = LabOrderStatus.Pending;
    public bool IsUrgent { get; set; } = false;

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Visit? Visit { get; set; }
    public ICollection<LabResult> Results { get; set; } = [];
}

public enum LabOrderStatus { Pending, Collected, Processing, Completed, Cancelled }
