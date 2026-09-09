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
        services.AddScoped<MedClinic.Application.Common.Interfaces.IWhatsAppProvider, MockWhatsAppProvider>();
        services.AddScoped<NotificationService>();
        services.AddScoped<MedClinic.Application.Interfaces.INotificationService>(sp => sp.GetRequiredService<NotificationService>());
        services.AddScoped<MedClinic.Application.Common.Interfaces.INotificationService>(sp => sp.GetRequiredService<NotificationService>());
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
