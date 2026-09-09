using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Exceptions;

public class ConsentRequiredException : Exception
{
    public ConsentType ConsentType { get; }

    public ConsentRequiredException(ConsentType consentType)
        : base($"Active patient consent for '{consentType}' is required before proceeding.")
    {
        ConsentType = consentType;
    }

    public ConsentRequiredException(ConsentType consentType, string message)
        : base(message)
    {
        ConsentType = consentType;
    }
}
