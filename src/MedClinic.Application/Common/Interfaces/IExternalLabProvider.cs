namespace MedClinic.Application.Common.Interfaces;

public interface IExternalLabProvider
{
    Task<ExternalLabOrderResult> SubmitOrderAsync(Guid patientId, Guid labOrderId, string payloadJson, CancellationToken ct = default);
    Task<ExternalLabResultFetch> FetchResultAsync(string externalOrderId, CancellationToken ct = default);
}

public record ExternalLabOrderResult(bool Success, string ExternalOrderId, string PayloadJson, string? ErrorMessage);
public record ExternalLabResultFetch(bool Success, string Status, string ResultJson, string? ErrorMessage);
