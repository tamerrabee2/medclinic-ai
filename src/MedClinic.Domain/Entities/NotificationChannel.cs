using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class NotificationLog : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? UserId { get; set; }

    public NotificationChannel Channel { get; set; }
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Recipient { get; set; } // phone / email / device token

    public NotificationDeliveryStatus DeliveryStatus { get; set; } = NotificationDeliveryStatus.Pending;
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
}

public enum NotificationChannel { InApp = 0, Email = 1, WhatsApp = 2, SMS = 3 }
public enum NotificationType
{
    AppointmentReminder = 0,
    AppointmentConfirmation = 1,
    AppointmentCancellation = 2,
    LabResultReady = 3,
    PrescriptionReady = 4,
    FollowUpDue = 5,
    GeneralAlert = 6,
    InvoiceReady = 7
}
public enum NotificationDeliveryStatus { Pending = 0, Sent = 1, Delivered = 2, Failed = 3, Unsubscribed = 4 }
