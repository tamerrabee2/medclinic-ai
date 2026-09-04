using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Prescription : TenantEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? VisitId { get; set; }
    public Visit? Visit { get; set; }
    public DateTime PrescribedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Clinic Clinic { get; set; } = null!;

    public ICollection<PrescriptionItem> Items { get; set; } = [];
}
