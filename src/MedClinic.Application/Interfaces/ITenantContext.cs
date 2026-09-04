namespace MedClinic.Application.Interfaces;

public interface ITenantContext
{
    Guid? ClinicId { get; }
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}
