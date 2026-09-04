using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class Doctor : TenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Specialty { get; set; } = string.Empty;
    public string? SubSpecialty { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Qualifications { get; set; }
    public int? ConsultationDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public Guid? UserId { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public ApplicationUser? User { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
}
