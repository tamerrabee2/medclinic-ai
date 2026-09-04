using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure.Audit;
using MedClinic.Infrastructure.BackgroundJobs;
using MedClinic.Infrastructure.Billing;
using MedClinic.Infrastructure.Notifications;
using MedClinic.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace MedClinic.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // ── Core Services ──
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService,        AuditService>();
        services.AddScoped<IFileStorage,         LocalFileStorage>();
        services.AddScoped<InvoiceStatusEngine>();

        // ── Background Jobs ──
        services.AddHostedService<OverdueInvoiceJob>();
        services.AddHostedService<AppointmentReminderJob>();
        services.AddHostedService<DataCleanupJob>();

        return services;
    }
}
