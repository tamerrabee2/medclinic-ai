namespace MedClinic.Application.Common.Interfaces;

public interface IFhirProvider
{
    Task<FhirExportResult> ExportPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<FhirExportResult> ExportObservationAsync(Guid patientId, Guid observationId, CancellationToken ct = default);
    Task<FhirExportResult> ExportEncounterAsync(Guid patientId, Guid encounterId, CancellationToken ct = default);
    Task<FhirImportResult> ImportPatientAsync(string resourceId, CancellationToken ct = default);
}

public record FhirExportResult(bool Success, string ResourceType, string ResourceId, string PayloadJson, string? ErrorMessage);
public record FhirImportResult(bool Success, string ResourceType, string ResourceId, string PayloadJson, string? ErrorMessage);
