namespace MedClinic.Shared.Common;

/// <summary>
/// Redacts free-text clinical and audit narratives in non-privileged / masked views
/// to prevent indirect patient PII or sensitive diagnostic leakage.
/// </summary>
public static class SensitiveNarrativeRedactor
{
    public const string DefaultRedactedPlaceholder = "[REDACTED — requires privileged export permission]";
    public const string DefaultViewRedactedPlaceholder = "[REDACTED — requires privileged view permission]";

    public static string Redact(
        string? narrative,
        bool maskPii,
        string? customPlaceholder = null)
    {
        if (string.IsNullOrWhiteSpace(narrative))
            return string.Empty;

        if (!maskPii)
            return narrative;

        return customPlaceholder ?? DefaultRedactedPlaceholder;
    }
}
