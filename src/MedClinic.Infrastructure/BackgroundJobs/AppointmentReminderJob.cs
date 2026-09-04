using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs every hour and sends reminders for appointments in the next 24 hours.
/// </summary>
public class AppointmentReminderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public AppointmentReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentReminderJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await SendRemindersAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "AppointmentReminderJob error."); }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        using var scope   = _scopeFactory.CreateScope();
        var db            = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now    = DateTime.UtcNow;
        var window = now.AddHours(24);

        // Appointments in the next 24h that haven’t been reminded yet
        var upcoming = await db.Appointments
            .Where(a =>
                a.ScheduledAt >= now    &&
                a.ScheduledAt <= window &&
                a.Status == AppointmentStatus.Confirmed &&
                !a.ReminderSent)
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .ToListAsync(ct);

        foreach (var appt in upcoming)
        {
            // Notify doctor
            await notifications.NotifyAsync(
                userId:     appt.Doctor.UserId,
                clinicId:   appt.ClinicId,
                title:      "Upcoming Appointment",
                body:       $"You have an appointment with {appt.Patient.FirstName} {appt.Patient.LastName} at {appt.ScheduledAt:HH:mm}.",
                type:       NotificationTypes.AppointmentReminder,
                entityType: "Appointment",
                entityId:   appt.Id,
                ct:         ct);

            appt.ReminderSent = true;
        }

        if (upcoming.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("AppointmentReminderJob sent {Count} reminders.", upcoming.Count);
        }
    }
}
