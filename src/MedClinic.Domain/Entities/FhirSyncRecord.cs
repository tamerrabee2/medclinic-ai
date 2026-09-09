using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class FhirSyncRecord : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string ResourceType { get; set; } = string.Empty; // Patient, Observation, DiagnosticReport, Encounter
    public string ResourceId { get; set; } = string.Empty;
    public string? ExternalSystem { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public FhirSyncDirection Direction { get; set; } = FhirSyncDirection.Export;
    public FhirSyncStatus Status { get; set; } = FhirSyncStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SyncedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

public enum FhirSyncDirection
{
    Import = 0,
    Export = 1
}

public enum FhirSyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2
}
