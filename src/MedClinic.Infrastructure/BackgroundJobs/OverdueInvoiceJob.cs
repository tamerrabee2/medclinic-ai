using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs daily and marks eligible invoices as Overdue.
/// Registered as a hosted service in Program.cs.
/// </summary>
public class OverdueInvoiceJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueInvoiceJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public OverdueInvoiceJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OverdueInvoiceJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverdueInvoiceJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MarkOverdueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OverdueInvoiceJob error.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task MarkOverdueAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;

        var invoices = await db.Invoices
            .Where(i =>
                i.DueDate < now &&
                (i.Status == InvoiceStatus.Sent ||
                 i.Status == InvoiceStatus.PartiallyPaid))
            .ToListAsync(ct);

        if (invoices.Count == 0) return;

        foreach (var inv in invoices)
            inv.Status = InvoiceStatus.Overdue;

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("OverdueInvoiceJob marked {Count} invoices as Overdue.", invoices.Count);
    }
}
