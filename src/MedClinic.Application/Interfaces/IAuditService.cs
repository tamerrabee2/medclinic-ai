namespace MedClinic.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        Guid    clinicId,
        Guid    userId,
        string  userName,
        string  entityName,
        Guid    entityId,
        string  action,
        object? oldValues  = null,
        object? newValues  = null,
        string? ipAddress  = null,
        CancellationToken ct = default);
}

public static class AuditActions
{
    public const string Created  = "Created";
    public const string Updated  = "Updated";
    public const string Deleted  = "Deleted";
    public const string Viewed   = "Viewed";
    public const string Exported = "Exported";
    public const string Login    = "Login";
    public const string Logout   = "Logout";
    public const string Signed   = "Signed";
    public const string Approved = "Approved";
    public const string Cancelled = "Cancelled";
}
