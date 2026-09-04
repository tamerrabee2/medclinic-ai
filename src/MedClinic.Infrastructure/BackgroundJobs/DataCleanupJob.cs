using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs weekly:
/// 1. Purges notifications older than 90 days that are already read.
/// 2. Purges audit logs older than 365 days.
/// 3. Purges expired refresh tokens.
/// </summary>
public class DataCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory    _scopeFactory;
    private readonly ILogger<DataCleanupJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromDays(7);

    public DataCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DataCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataCleanupJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunCleanupAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "DataCleanupJob error."); }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now         = DateTime.UtcNow;

        // 1. Remove read notifications older than 90 days
        var notifCutoff = now.AddDays(-90);
        var deletedNotifs = await db.Notifications
            .Where(n => n.IsRead && n.CreatedAt < notifCutoff)
            .ExecuteDeleteAsync(ct);

        // 2. Remove audit logs older than 365 days
        var auditCutoff = now.AddDays(-365);
        var deletedAudit = await db.AuditLogs
            .Where(a => a.CreatedAt < auditCutoff)
            .ExecuteDeleteAsync(ct);

        // 3. Remove expired refresh tokens
        var deletedTokens = await db.RefreshTokens
            .Where(t => t.ExpiresAt < now)
            .ExecuteDeleteAsync(ct);

        _logger.LogInformation(
            "DataCleanupJob: removed {Notifs} notifications, {Audit} audit logs, {Tokens} refresh tokens.",
            deletedNotifs, deletedAudit, deletedTokens);
    }
}
