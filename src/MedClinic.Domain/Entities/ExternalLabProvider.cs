using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class ExternalLabProvider : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string? AuthType { get; set; }
    public bool IsActive { get; set; } = true;

    public Clinic Clinic { get; set; } = null!;
}

public class ExternalLabSync : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? LabOrderId { get; set; }
    public Guid ExternalLabProviderId { get; set; }
    public string ExternalOrderId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string PayloadJson { get; set; } = "{}";
    public string? ResultJson { get; set; }
    public DateTime? SyncedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ExternalLabProvider Provider { get; set; } = null!;
}
