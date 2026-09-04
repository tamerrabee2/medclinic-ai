namespace MedClinic.Application.Interfaces;

public interface ITenantContext
{
    Guid? ClinicId { get; }
    Guid? UserId { get; }
    string? UserEmail { get; }
    string? UserRole { get; }
    bool HasPermission(string permission);
}
