using MedClinic.Application.Common.Models;
using MedClinic.Application.Features.Subscriptions.DTOs;
using MedClinic.Domain.Enums;

namespace MedClinic.Application.Interfaces;

public interface ITenantEntitlementService
{
    /// <summary>
    /// Checks whether the tenant is entitled to use a specific feature key.
    /// </summary>
    Task<bool> HasFeatureAsync(Guid clinicId, string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Gets all active feature keys enabled for the clinic.
    /// </summary>
    Task<IReadOnlyList<string>> GetActiveFeaturesAsync(Guid clinicId, CancellationToken ct = default);

    /// <summary>
    /// Evaluates if the clinic is within its quota for a specific metric.
    /// </summary>
    Task<EntitlementResult> CheckQuotaAsync(Guid clinicId, MetricType metricType, CancellationToken ct = default);

    /// <summary>
    /// Enforces the Tenant Suspension Access Matrix. Allows read-only & compliance actions
    /// during suspension, while blocking operational mutations (appointments, visits, AI, etc.).
    /// </summary>
    Task<EntitlementResult> CheckCanExecuteAsync(Guid clinicId, ClinicalAction action, CancellationToken ct = default);

    /// <summary>
    /// Records usage atomically for the active period bucket.
    /// </summary>
    Task RecordUsageAsync(Guid clinicId, MetricType metricType, long amount = 1, CancellationToken ct = default);

    /// <summary>
    /// Returns the comprehensive subscription, lifecycle status, and quota breakdown for a clinic.
    /// </summary>
    Task<ClinicSubscriptionSummaryDto> GetSubscriptionSummaryAsync(Guid clinicId, CancellationToken ct = default);
}
