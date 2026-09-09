using MedClinic.Domain.Entities;

namespace MedClinic.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken ct = default);
    Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken ct = default);
    Task SendLabResultReadyAsync(LabOrder labOrder, CancellationToken ct = default);
    Task SendFollowUpReminderAsync(FollowUpIntelligence followUp, CancellationToken ct = default);
    Task SendInvoiceReadyAsync(Invoice invoice, CancellationToken ct = default);
    Task SendCustomAsync(Guid clinicId, Guid? patientId, NotificationType type,
        NotificationChannel channel, string recipient, string subject, string body,
        CancellationToken ct = default);
}

public interface IWhatsAppProvider
{
    Task<string?> SendMessageAsync(string toPhone, string message, CancellationToken ct = default);
    Task<string?> SendTemplateAsync(string toPhone, string templateName,
        Dictionary<string, string> parameters, CancellationToken ct = default);
}
