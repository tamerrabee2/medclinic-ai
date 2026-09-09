using System.Text.Json;
using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Insurance;

public class MockInsuranceProviderGateway : IInsuranceProviderGateway
{
    private readonly ILogger<MockInsuranceProviderGateway> _logger;

    public MockInsuranceProviderGateway(ILogger<MockInsuranceProviderGateway> logger) => _logger = logger;

    public Task<InsuranceClaimSubmissionResult> SubmitClaimAsync(Guid patientId, Guid invoiceId, string payloadJson, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockInsurance] Submit claim for invoice {InvoiceId}", invoiceId);
        return Task.FromResult(new InsuranceClaimSubmissionResult(true, $"CLM-{invoiceId:N}", payloadJson, null));
    }

    public Task<InsuranceClaimStatusResult> CheckClaimStatusAsync(string claimNumber, CancellationToken ct = default)
    {
        var response = JsonSerializer.Serialize(new { claimNumber, status = "Approved", approvedAmount = 350m });
        return Task.FromResult(new InsuranceClaimStatusResult(true, "Approved", 350m, response, null));
    }
}
