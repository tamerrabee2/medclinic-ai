using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class PatientPortalAccess : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

public class PatientPortalMessage : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? UserId { get; set; }
    public string SenderType { get; set; } = string.Empty; // Patient / Clinic
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
