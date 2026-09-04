namespace MedClinic.Application.Interfaces;

public interface ITenantContext
{
    Guid? ClinicId { get; }
    Guid? UserId { get; }
    string? ClinicName { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
    bool IsSuperAdmin { get; }
}
