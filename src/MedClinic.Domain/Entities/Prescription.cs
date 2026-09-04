using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Prescription : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? VisitId { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string? DiagnosisSummary { get; set; }
    public string? Notes { get; set; }
    public bool IsSigned { get; set; } = false;
    public string? PdfUrl { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Visit? Visit { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = [];
}
