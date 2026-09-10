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
