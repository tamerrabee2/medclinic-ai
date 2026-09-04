using MedClinic.Domain.Entities;

namespace MedClinic.Application.Interfaces;

public interface INotificationService
{
    /// <summary>Create an in-app notification for a specific user</summary>
    Task NotifyAsync(
        Guid   userId,
        Guid   clinicId,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid?   entityId   = null,
        CancellationToken ct = default);

    /// <summary>Notify all members of a clinic by role</summary>
    Task NotifyRoleAsync(
        Guid   clinicId,
        string role,
        string title,
        string body,
        string type,
        string? entityType = null,
        Guid?   entityId   = null,
        CancellationToken ct = default);

    /// <summary>Notify multiple users at once</summary>
    Task NotifyManyAsync(
        IEnumerable<Guid> userIds,
        Guid              clinicId,
        string            title,
        string            body,
        string            type,
        string?           entityType = null,
        Guid?             entityId   = null,
        CancellationToken ct = default);
}

public static class NotificationTypes
{
    public const string AppointmentReminder  = "appointment.reminder";
    public const string AppointmentBooked    = "appointment.booked";
    public const string AppointmentCancelled = "appointment.cancelled";
    public const string AppointmentUpdated   = "appointment.updated";
    public const string LabResultReady       = "lab.result.ready";
    public const string RadiologyReportReady = "radiology.report.ready";
    public const string InvoiceSent          = "invoice.sent";
    public const string PaymentReceived      = "payment.received";
    public const string PaymentOverdue       = "payment.overdue";
    public const string PrescriptionSigned   = "prescription.signed";
    public const string SystemAlert          = "system.alert";
}
