using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabOrder : TenantEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? VisitId { get; set; }
    public Visit? Visit { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public Clinic Clinic { get; set; } = null!;

    public ICollection<LabResult> Results { get; set; } = [];
}
