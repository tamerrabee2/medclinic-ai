using System.Text.Json;
using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.ExternalLabs;

public class MockExternalLabProvider : IExternalLabProvider
{
    private readonly ILogger<MockExternalLabProvider> _logger;

    public MockExternalLabProvider(ILogger<MockExternalLabProvider> logger) => _logger = logger;

    public Task<ExternalLabOrderResult> SubmitOrderAsync(Guid patientId, Guid labOrderId, string payloadJson, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockExternalLab] Submit lab order {LabOrderId} for patient {PatientId}", labOrderId, patientId);
        return Task.FromResult(new ExternalLabOrderResult(true, $"ext-lab-{labOrderId:N}", payloadJson, null));
    }

    public Task<ExternalLabResultFetch> FetchResultAsync(string externalOrderId, CancellationToken ct = default)
    {
        var result = JsonSerializer.Serialize(new
        {
            externalOrderId,
            status = "Completed",
            items = new[]
            {
                new { test = "CBC", value = "Normal" },
                new { test = "HbA1c", value = "7.1%" }
            }
        });
        return Task.FromResult(new ExternalLabResultFetch(true, "Completed", result, null));
    }
}
