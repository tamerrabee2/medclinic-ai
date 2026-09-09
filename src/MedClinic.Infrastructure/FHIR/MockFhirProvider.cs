using System.Text.Json;
using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.FHIR;

public class MockFhirProvider : IFhirProvider
{
    private readonly ILogger<MockFhirProvider> _logger;

    public MockFhirProvider(ILogger<MockFhirProvider> logger) => _logger = logger;

    public Task<FhirExportResult> ExportPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        var resource = new
        {
            resourceType = "Patient",
            id = $"patient-{patientId:N}",
            active = true,
            identifier = new[] { new { system = "urn:medclinic:patient", value = patientId.ToString() } }
        };
        var json = JsonSerializer.Serialize(resource);
        _logger.LogInformation("[MockFHIR] Export patient {PatientId}", patientId);
        return Task.FromResult(new FhirExportResult(true, "Patient", $"patient-{patientId:N}", json, null));
    }

    public Task<FhirExportResult> ExportObservationAsync(Guid patientId, Guid observationId, CancellationToken ct = default)
    {
        var resource = new
        {
            resourceType = "Observation",
            id = $"observation-{observationId:N}",
            status = "final",
            subject = new { reference = $"Patient/patient-{patientId:N}" }
        };
        var json = JsonSerializer.Serialize(resource);
        return Task.FromResult(new FhirExportResult(true, "Observation", $"observation-{observationId:N}", json, null));
    }

    public Task<FhirExportResult> ExportEncounterAsync(Guid patientId, Guid encounterId, CancellationToken ct = default)
    {
        var resource = new
        {
            resourceType = "Encounter",
            id = $"encounter-{encounterId:N}",
            status = "finished",
            subject = new { reference = $"Patient/patient-{patientId:N}" }
        };
        var json = JsonSerializer.Serialize(resource);
        return Task.FromResult(new FhirExportResult(true, "Encounter", $"encounter-{encounterId:N}", json, null));
    }

    public Task<FhirImportResult> ImportPatientAsync(string resourceId, CancellationToken ct = default)
    {
        var resource = new
        {
            resourceType = "Patient",
            id = resourceId,
            active = true
        };
        var json = JsonSerializer.Serialize(resource);
        _logger.LogInformation("[MockFHIR] Import patient resource {ResourceId}", resourceId);
        return Task.FromResult(new FhirImportResult(true, "Patient", resourceId, json, null));
    }
}
