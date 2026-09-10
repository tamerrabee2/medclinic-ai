using MedClinic.Domain.Enums;

namespace MedClinic.Application.Common.Models;

public sealed class QuotaReservationResult
{
    public bool IsAllowed { get; init; }
    public Guid? ReservationId { get; init; }
    public string? Code { get; init; }
    public string? Reason { get; init; }
    public MetricType? MetricType { get; init; }
    public long CurrentUsage { get; init; }
    public long ActiveReserved { get; init; }
    public long QuotaLimit { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }

    public static QuotaReservationResult Success(Guid reservationId, MetricType metric, long currentUsage, long activeReserved, long limit, DateTime expiresAtUtc) => new()
    {
        IsAllowed = true,
        ReservationId = reservationId,
        MetricType = metric,
        CurrentUsage = currentUsage,
        ActiveReserved = activeReserved,
        QuotaLimit = limit,
        ExpiresAtUtc = expiresAtUtc
    };

    public static QuotaReservationResult QuotaExceeded(MetricType metric, long currentUsage, long activeReserved, long limit, string? reason = null) => new()
    {
        IsAllowed = false,
        Code = "quota_exceeded",
        MetricType = metric,
        CurrentUsage = currentUsage,
        ActiveReserved = activeReserved,
        QuotaLimit = limit,
        Reason = reason ?? $"Quota limit of {limit} for {metric} would be exceeded (current committed: {currentUsage}, active reserved: {activeReserved})."
    };

    public static QuotaReservationResult Suspended(string? reason = null) => new()
    {
        IsAllowed = false,
        Code = "clinic_suspended",
        Reason = reason ?? "Clinic operations are suspended. Operational reservations cannot be issued."
    };

    public static QuotaReservationResult FeatureNotIncluded(string featureKey, string? reason = null) => new()
    {
        IsAllowed = false,
        Code = "feature_not_included",
        Reason = reason ?? $"Feature '{featureKey}' is not included in the active subscription plan."
    };
}
