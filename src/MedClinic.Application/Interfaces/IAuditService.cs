namespace MedClinic.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string action,
        Guid? userId = null,
        Guid? clinicId = null,
        string? entityName = null,
        Guid? entityId = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}
