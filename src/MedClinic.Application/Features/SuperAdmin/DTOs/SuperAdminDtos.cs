using MedClinic.Domain.Enums;

namespace MedClinic.Application.Features.SuperAdmin.DTOs;

public sealed record ProvisionClinicRequest(
    string Name,
    string Slug,
    string? Description,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string Currency = "USD",
    string Timezone = "UTC",
    string PlanCode = "pro",
    BillingCycle BillingCycle = BillingCycle.Monthly,
    int TrialDays = 14,
    AdminUserProvisionDto? AdminUser = null
);

public sealed record AdminUserProvisionDto(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? PhoneNumber = null
);

public sealed record TenantLifecycleAuditEventDto(
    Guid Id,
    Guid ClinicId,
    TenantLifecycleEventType EventType,
    string EventTypeName,
    string? OldValue,
    string? NewValue,
    string? Reason,
    Guid? PerformedByUserId,
    string? PerformedByUserName,
    DateTime Timestamp,
    string? IpAddress,
    string? CorrelationId
);

public sealed record SuspendClinicRequest(
    string Reason,
    string? SuspensionCode = null
);

public sealed record ReactivateClinicRequest(
    string? Reason = null
);

public sealed record ChangeClinicPlanRequest(
    Guid NewPlanId,
    BillingCycle BillingCycle = BillingCycle.Monthly
);

public sealed record SetClinicComplianceRequest(
    TenantComplianceStatus Status,
    string Reason
);

public sealed record SuperAdminOverviewDto(
    int TotalClinics,
    int ActiveClinics,
    int TrialClinics,
    int SuspendedClinics,
    int CancelledClinics,
    int TotalDoctors,
    int TotalPatients,
    long TotalAiRequests,
    long TotalDicomStudies,
    IReadOnlyDictionary<string, int> ClinicsByTier
);

public sealed record TenantListItemDto(
    Guid Id,
    string Name,
    string Slug,
    string? Email,
    string? Phone,
    string? City,
    string? Country,
    ClinicLifecycleStatus LifecycleStatus,
    BillingStatus BillingStatus,
    TenantComplianceStatus ComplianceStatus,
    DateTime? TrialEndsAt,
    DateTime? SuspendedAt,
    string? SuspensionReason,
    string PlanCode,
    SubscriptionTier Tier,
    int DoctorsCount,
    int PatientsCount,
    DateTime CreatedAt
);

public sealed record CreateOrUpdatePlanRequest(
    string Code,
    string Name,
    string? Description,
    SubscriptionTier Tier,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    string Currency,
    int MaxDoctors,
    int MaxUsers,
    int MaxPatients,
    long MaxStorageBytes,
    int MonthlyAiRequestsLimit,
    int MaxDicomStudiesMonthly,
    List<string> Features,
    bool IsActive = true,
    int DisplayOrder = 0
);
