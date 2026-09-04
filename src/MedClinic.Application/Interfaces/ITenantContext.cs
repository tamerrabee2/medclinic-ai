namespace MedClinic.Application.Interfaces;

public interface ITenantContext
{
    Guid? ClinicId { get; }
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    IList<string> Roles { get; }
}
