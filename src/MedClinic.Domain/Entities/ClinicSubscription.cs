using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class ClinicSubscription : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }

    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Basic;
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Current;

    public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndDateUtc { get; set; }
    public DateTime? NextBillingDateUtc { get; set; }
    public DateTime? GracePeriodEndsAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    public bool IsActive { get; set; } = true;
    public bool AutoRenew { get; set; } = true;

    // ── Immutable Snapshots of Agreed Plan Terms ───────────────────────────
    public string PlanCodeSnapshot { get; set; } = string.Empty;
    public SubscriptionTier TierSnapshot { get; set; }
    public decimal MonthlyPriceSnapshot { get; set; }
    public decimal AnnualPriceSnapshot { get; set; }
    public string CurrencySnapshot { get; set; } = "USD";
    public string FeaturesSnapshot { get; set; } = "[]";
    public int MaxDoctorsSnapshot { get; set; }
    public int MaxUsersSnapshot { get; set; }
    public int MaxPatientsSnapshot { get; set; }
    public long MaxStorageBytesSnapshot { get; set; }
    public int MonthlyAiRequestsLimitSnapshot { get; set; }
    public int MaxDicomStudiesMonthlySnapshot { get; set; }

    public static ClinicSubscription CreateWithSnapshot(
        Guid clinicId,
        SubscriptionPlan plan,
        BillingCycle cycle,
        SubscriptionStatus status = SubscriptionStatus.Active,
        BillingStatus billingStatus = BillingStatus.Current,
        DateTime? endDateUtc = null)
    {
        return new ClinicSubscription
        {
            ClinicId = clinicId,
            SubscriptionPlanId = plan.Id,
            Tier = plan.Tier,
            BillingCycle = cycle,
            Status = status,
            BillingStatus = billingStatus,
            StartDateUtc = DateTime.UtcNow,
            EndDateUtc = endDateUtc,
            NextBillingDateUtc = endDateUtc,
            IsActive = true,
            AutoRenew = true,
            PlanCodeSnapshot = plan.Code,
            TierSnapshot = plan.Tier,
            MonthlyPriceSnapshot = plan.MonthlyPrice,
            AnnualPriceSnapshot = plan.AnnualPrice,
            CurrencySnapshot = plan.Currency,
            FeaturesSnapshot = plan.FeaturesJson,
            MaxDoctorsSnapshot = plan.MaxDoctors,
            MaxUsersSnapshot = plan.MaxUsers,
            MaxPatientsSnapshot = plan.MaxPatients,
            MaxStorageBytesSnapshot = plan.MaxStorageBytes,
            MonthlyAiRequestsLimitSnapshot = plan.MonthlyAiRequestsLimit,
            MaxDicomStudiesMonthlySnapshot = plan.MaxDicomStudiesMonthly
        };
    }
}
