using MedClinic.Domain.Enums;

namespace MedClinic.Application.Features.Subscriptions.DTOs;

public sealed record ClinicSubscriptionSummaryDto(
    Guid ClinicId,
    string ClinicName,
    ClinicLifecycleStatus LifecycleStatus,
    BillingStatus BillingStatus,
    TenantComplianceStatus ComplianceStatus,
    string? SuspensionReason,
    string PlanCode,
    string PlanName,
    SubscriptionTier Tier,
    BillingCycle BillingCycle,
    SubscriptionStatus SubscriptionStatus,
    DateTime? TrialEndsAt,
    DateTime? NextBillingDateUtc,
    DateTime? GracePeriodEndsAtUtc,
    IReadOnlyList<string> EnabledFeatures,
    IReadOnlyList<UsageQuotaDto> Quotas
);

public sealed record UsageQuotaDto(
    MetricType MetricType,
    string MetricName,
    long CurrentValue,
    long QuotaLimit,
    double UsagePercentage,
    bool IsExceeded,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc
);
