namespace MedClinic.Application.Common.Interfaces;

public interface IInsuranceProviderGateway
{
    Task<InsuranceClaimSubmissionResult> SubmitClaimAsync(Guid patientId, Guid invoiceId, string payloadJson, CancellationToken ct = default);
    Task<InsuranceClaimStatusResult> CheckClaimStatusAsync(string claimNumber, CancellationToken ct = default);
}

public record InsuranceClaimSubmissionResult(bool Success, string ClaimNumber, string PayloadJson, string? ErrorMessage);
public record InsuranceClaimStatusResult(bool Success, string Status, decimal ApprovedAmount, string ResponseJson, string? ErrorMessage);
