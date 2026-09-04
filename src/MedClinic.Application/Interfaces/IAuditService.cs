namespace MedClinic.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}
