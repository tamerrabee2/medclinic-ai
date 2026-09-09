using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Notifications;

public class NotificationService : INotificationService, MedClinic.Application.Interfaces.INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IWhatsAppProvider _whatsApp;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        IWhatsAppProvider whatsApp,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _whatsApp = whatsApp;
        _logger = logger;
    }

    // ─── In-App Notifications (MedClinic.Application.Interfaces.INotificationService) ───

    public async Task NotifyAsync(
        Guid userId,
        Guid clinicId,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            ClinicId = clinicId,
            Title = title,
            Body = body,
            Type = type,
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task NotifyRoleAsync(
        Guid clinicId,
        string role,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid? entityId = null,
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
        Guid clinicId,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken ct = default)
    {
        var notifications = userIds.Select(uid => new Notification
        {
            UserId = uid,
            ClinicId = clinicId,
            Title = title,
            Body = body,
            Type = type,
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Count > 0)
        {
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync(ct);
        }
    }

    // ─── Multi-Channel Notifications (MedClinic.Application.Common.Interfaces.INotificationService) ───

    public async Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken ct = default)
    {
        var phone = appointment.Patient?.Phone;
        if (string.IsNullOrEmpty(phone)) return;

        var message = $"Reminder: You have an appointment on {appointment.ScheduledAt:dd/MM/yyyy} at {appointment.ScheduledAt:HH:mm}. Please arrive 10 minutes early.";

        await LogAndSendAsync(appointment.ClinicId, appointment.PatientId,
            NotificationType.AppointmentReminder, NotificationChannel.WhatsApp,
            phone, "Appointment Reminder", message, ct);
    }

    public async Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken ct = default)
    {
        var phone = appointment.Patient?.Phone;
        if (string.IsNullOrEmpty(phone)) return;

        var message = $"Your appointment on {appointment.ScheduledAt:dd/MM/yyyy} at {appointment.ScheduledAt:HH:mm} is confirmed.";

        await LogAndSendAsync(appointment.ClinicId, appointment.PatientId,
            NotificationType.AppointmentConfirmation, NotificationChannel.WhatsApp,
            phone, "Appointment Confirmed", message, ct);
    }

    public async Task SendLabResultReadyAsync(LabOrder labOrder, CancellationToken ct = default)
    {
        var phone = labOrder.Patient?.Phone;
        if (string.IsNullOrEmpty(phone)) return;

        await LogAndSendAsync(labOrder.ClinicId, labOrder.PatientId,
            NotificationType.LabResultReady, NotificationChannel.WhatsApp,
            phone, "Lab Results Ready", "Your laboratory results are ready. Please contact your clinic.", ct);
    }

    public async Task SendFollowUpReminderAsync(FollowUpIntelligence followUp, CancellationToken ct = default)
    {
        _logger.LogInformation("Follow-up reminder for patient {PatientId}", followUp.PatientId);
        await Task.CompletedTask;
    }

    public async Task SendInvoiceReadyAsync(Invoice invoice, CancellationToken ct = default)
    {
        _logger.LogInformation("Invoice ready notification for {InvoiceId}", invoice.Id);
        await Task.CompletedTask;
    }

    public async Task SendCustomAsync(Guid clinicId, Guid? patientId, NotificationType type,
        NotificationChannel channel, string recipient, string subject, string body,
        CancellationToken ct = default)
    {
        await LogAndSendAsync(clinicId, patientId, type, channel, recipient, subject, body, ct);
    }

    private async Task LogAndSendAsync(Guid clinicId, Guid? patientId, NotificationType type,
        NotificationChannel channel, string recipient, string subject, string body,
        CancellationToken ct)
    {
        var log = new NotificationLog
        {
            ClinicId = clinicId,
            PatientId = patientId,
            Type = type,
            Channel = channel,
            Recipient = recipient,
            Subject = subject,
            Body = body,
            DeliveryStatus = NotificationDeliveryStatus.Pending
        };

        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        try
        {
            if (channel == NotificationChannel.WhatsApp)
            {
                var msgId = await _whatsApp.SendMessageAsync(recipient, body, ct);
                log.ProviderMessageId = msgId;
                log.DeliveryStatus = msgId != null
                    ? NotificationDeliveryStatus.Sent
                    : NotificationDeliveryStatus.Failed;
                log.SentAt = msgId != null ? DateTime.UtcNow : null;
            }

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            log.DeliveryStatus = NotificationDeliveryStatus.Failed;
            log.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(ct);
            _logger.LogError(ex, "Notification send failed for {Channel} to {Recipient}", channel, recipient);
        }
    }
}
