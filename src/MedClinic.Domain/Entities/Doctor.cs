using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Doctor : TenantEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Specialization { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public string? Qualifications { get; set; }
    public string? Bio { get; set; }
    public int? ConsultationFee { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Clinic Clinic { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
}
