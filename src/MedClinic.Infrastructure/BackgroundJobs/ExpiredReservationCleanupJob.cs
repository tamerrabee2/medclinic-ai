using MedClinic.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that periodically transitions stale Reserved usage events past their expiry to Expired.
/// </summary>
public class ExpiredReservationCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredReservationCleanupJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public ExpiredReservationCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredReservationCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredReservationCleanupJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var entitlement = scope.ServiceProvider.GetRequiredService<ITenantEntitlementService>();
                var cleanedCount = await entitlement.CleanupExpiredReservationsAsync(stoppingToken);

                if (cleanedCount > 0)
                {
                    _logger.LogInformation("ExpiredReservationCleanupJob: Cleaned up {Count} expired reservation(s).", cleanedCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred during ExpiredReservationCleanupJob execution.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
