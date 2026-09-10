namespace MedClinic.Domain.Enums;

public enum TenantLifecycleEventType
{
    Onboarded = 1,
    TrialStarted = 2,
    TrialExpired = 3,
    Suspended = 4,
    Reactivated = 5,
    PlanChanged = 6,
    BillingStatusChanged = 7,
    QuotaOverridden = 8,
    Cancelled = 9
}
