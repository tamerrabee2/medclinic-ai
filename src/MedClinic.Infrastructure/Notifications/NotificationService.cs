using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
        => _context = context;

    public async Task NotifyAsync(
        Guid   userId,
        Guid   clinicId,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid?   entityId   = null,
        CancellationToken ct = default)
    {
        _context.Notifications.Add(new Notification
        {
            UserId     = userId,
            ClinicId   = clinicId,
            Title      = title,
            Body       = body,
            Type       = type,
            EntityType = entityType,
            EntityId   = entityId,
            IsRead     = false
        });
        await _context.SaveChangesAsync(ct);
    }

    public async Task NotifyRoleAsync(
        Guid   clinicId,
        string role,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid?   entityId   = null,
        CancellationToken ct = default)
    {
        var userIds = await _context.ClinicMembers
            .Where(m => m.ClinicId == clinicId && m.Role == role)
            .Select(m => m.UserId)
            .ToListAsync(ct);

        await NotifyManyAsync(userIds, clinicId, title, body, type, entityType, entityId, ct);
    }

    public async Task NotifyManyAsync(
        IEnumerable<Guid> userIds,
        Guid              clinicId,
        string            title,
        string            body,
        string            type,
        string?           entityType = null,
        Guid?             entityId   = null,
        CancellationToken ct = default)
    {
        var notifications = userIds.Select(uid => new Notification
        {
            UserId     = uid,
            ClinicId   = clinicId,
            Title      = title,
            Body       = body,
            Type       = type,
            EntityType = entityType,
            EntityId   = entityId,
            IsRead     = false
        }).ToList();

        if (notifications.Count == 0) return;

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }
}
