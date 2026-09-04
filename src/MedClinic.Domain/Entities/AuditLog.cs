using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid    ClinicId   { get; set; }
    public Guid    UserId     { get; set; }
    public string  UserName   { get; set; } = string.Empty;
    public string  EntityName { get; set; } = string.Empty;  // "Patient", "Invoice"
    public Guid    EntityId   { get; set; }
    public string  Action     { get; set; } = string.Empty;  // see AuditActions
    public string? OldValues  { get; set; }  // JSON snapshot before
    public string? NewValues  { get; set; }  // JSON snapshot after
    public string? IpAddress  { get; set; }
}
