using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Notifications;

public class NotificationService : INotificationService
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

    public async Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken ct = default)
    {
        var phone = appointment.Patient?.PhoneNumber;
        if (string.IsNullOrEmpty(phone)) return;

        var message = $"Reminder: You have an appointment on {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.AppointmentDate:HH:mm}. Please arrive 10 minutes early.";

        await LogAndSendAsync(appointment.ClinicId, appointment.PatientId,
            NotificationType.AppointmentReminder, NotificationChannel.WhatsApp,
            phone, "Appointment Reminder", message, ct);
    }

    public async Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken ct = default)
    {
        var phone = appointment.Patient?.PhoneNumber;
        if (string.IsNullOrEmpty(phone)) return;

        var message = $"Your appointment on {appointment.AppointmentDate:dd/MM/yyyy} at {appointment.AppointmentDate:HH:mm} is confirmed.";

        await LogAndSendAsync(appointment.ClinicId, appointment.PatientId,
            NotificationType.AppointmentConfirmation, NotificationChannel.WhatsApp,
            phone, "Appointment Confirmed", message, ct);
    }

    public async Task SendLabResultReadyAsync(LabOrder labOrder, CancellationToken ct = default)
    {
        var phone = labOrder.Patient?.PhoneNumber;
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
