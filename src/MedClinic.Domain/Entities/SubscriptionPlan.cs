using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g. "basic", "pro", "enterprise"
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Basic;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int MaxDoctors { get; set; }
    public int MaxUsers { get; set; }
    public int MaxPatients { get; set; }
    public long MaxStorageBytes { get; set; }
    public int MonthlyAiRequestsLimit { get; set; }
    public int MaxDicomStudiesMonthly { get; set; }
    public string FeaturesJson { get; set; } = "[]"; // Serialized list of FeatureKey strings
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    public ICollection<ClinicSubscription> Subscriptions { get; set; } = [];
}
