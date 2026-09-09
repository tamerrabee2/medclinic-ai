using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Enums;
using MedClinic.Domain.Exceptions;

namespace MedClinic.Infrastructure.Services;

public sealed class AiConsentGuard : IAiConsentGuard
{
    private readonly IConsentService _consentService;

    public AiConsentGuard(IConsentService consentService)
    {
        _consentService = consentService;
    }

    public async Task EnsureAiConsentAsync(Guid patientId, CancellationToken ct = default)
    {
        var hasConsent = await _consentService.HasActiveConsentAsync(patientId, ConsentType.AiAssistedCare, ct);
        if (!hasConsent)
        {
            throw new ConsentRequiredException(
                ConsentType.AiAssistedCare,
                $"Active patient consent for '{ConsentType.AiAssistedCare}' is required before executing clinical AI operations.");
        }
    }
}
