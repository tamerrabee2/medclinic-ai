using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class ClinicMember : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Clinic Clinic { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
