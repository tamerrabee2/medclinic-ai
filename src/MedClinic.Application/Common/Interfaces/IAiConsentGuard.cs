namespace MedClinic.Application.Common.Interfaces;

/// <summary>
/// Enforces mandatory patient consent validation prior to executing sensitive clinical AI workflows.
/// </summary>
public interface IAiConsentGuard
{
    Task EnsureAiConsentAsync(Guid patientId, CancellationToken ct = default);
}
