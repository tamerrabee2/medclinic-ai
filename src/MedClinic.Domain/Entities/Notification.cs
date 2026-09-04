using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid    UserId      { get; set; }
    public Guid    ClinicId    { get; set; }
    public string  Title       { get; set; } = string.Empty;
    public string  Body        { get; set; } = string.Empty;
    public string  Type        { get; set; } = string.Empty; // see NotificationTypes
    public bool    IsRead      { get; set; } = false;
    public DateTime? ReadAt    { get; set; }
    public string? EntityType  { get; set; }  // e.g. "Appointment", "Invoice"
    public Guid?   EntityId    { get; set; }  // foreign key to entity
}
