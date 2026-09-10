using MedClinic.Domain.Enums;

namespace MedClinic.Application.Common.Models;

public sealed class EntitlementResult
{
    public bool IsAllowed { get; init; }
    public string? Code { get; init; }
    public string? Reason { get; init; }
    public MetricType? MetricType { get; init; }
    public long? CurrentValue { get; init; }
    public long? QuotaLimit { get; init; }

    public static EntitlementResult Success() => new() { IsAllowed = true };

    public static EntitlementResult Suspended(string? reason = null) => new()
    {
        IsAllowed = false,
        Code = "clinic_suspended",
        Reason = reason ?? "Clinic operations are suspended. Only read-only medical records and compliance operations are permitted."
    };

    public static EntitlementResult PastDueGracePeriod(string? reason = null) => new()
    {
        IsAllowed = true,
        Code = "past_due_grace_period",
        Reason = reason ?? "Account payment is past due; currently in grace period."
    };

    public static EntitlementResult QuotaExceeded(MetricType metric, long current, long limit, string? reason = null) => new()
    {
        IsAllowed = false,
        Code = "quota_exceeded",
        MetricType = metric,
        CurrentValue = current,
        QuotaLimit = limit,
        Reason = reason ?? $"Monthly quota limit of {limit} for {metric} has been reached (current: {current})."
    };

    public static EntitlementResult FeatureNotIncluded(string featureKey, string? reason = null) => new()
    {
        IsAllowed = false,
        Code = "feature_not_included",
        Reason = reason ?? $"Feature '{featureKey}' is not included in the active subscription tier."
    };
}
