using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using System.Text.Json;

namespace MedClinic.Infrastructure.Audit;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
        => _context = context;

    public async Task LogAsync(
        Guid    clinicId,
        Guid    userId,
        string  userName,
        string  entityName,
        Guid    entityId,
        string  action,
        object? oldValues  = null,
        object? newValues  = null,
        string? ipAddress  = null,
        CancellationToken ct = default)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            ClinicId   = clinicId,
            UserId     = userId,
            UserName   = userName,
            EntityName = entityName,
            EntityId   = entityId,
            Action     = action,
            OldValues  = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues  = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress  = ipAddress
        });
        await _context.SaveChangesAsync(ct);
    }
}
