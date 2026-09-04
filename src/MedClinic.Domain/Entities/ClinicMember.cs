using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class ClinicMember : TenantEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Clinic Clinic { get; set; } = null!;
}
