using FluentAssertions;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Shared.Constants;
using Xunit;

namespace MedClinic.Tests.Unit.Domain;

public class SaasFoundationDomainTests
{
    [Fact]
    public void Clinic_ShouldDefaultToTrial_CurrentBilling_AndNormalCompliance()
    {
        var clinic = new Clinic
        {
            Name = "Al-Amal Medical Center",
            Slug = "al-amal"
        };

        clinic.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Trial);
        clinic.BillingStatus.Should().Be(BillingStatus.Current);
        clinic.ComplianceStatus.Should().Be(TenantComplianceStatus.Normal);
        clinic.TrialEndsAt.Should().BeNull();
        clinic.SuspendedAt.Should().BeNull();
    }

    [Fact]
    public void ClinicSubscription_CreateWithSnapshot_ShouldLockPlanTermsPermanently()
    {
        var clinicId = Guid.NewGuid();
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = "pro",
            Name = "Professional Clinic",
            Tier = SubscriptionTier.Pro,
            MonthlyPrice = 149.00m,
            AnnualPrice = 1490.00m,
            Currency = "USD",
            MaxDoctors = 15,
            MaxUsers = 25,
            MaxPatients = 5000,
            MaxStorageBytes = 50L * 1024 * 1024 * 1024,
            MonthlyAiRequestsLimit = 1500,
            MaxDicomStudiesMonthly = 100,
            FeaturesJson = "[\"ai_copilot\",\"dicom_pacs\"]"
        };

        var subscription = ClinicSubscription.CreateWithSnapshot(
            clinicId,
            plan,
            BillingCycle.Annual,
            SubscriptionStatus.Active,
            BillingStatus.Current);

        subscription.ClinicId.Should().Be(clinicId);
        subscription.SubscriptionPlanId.Should().Be(plan.Id);
        subscription.Tier.Should().Be(SubscriptionTier.Pro);
        subscription.BillingCycle.Should().Be(BillingCycle.Annual);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.BillingStatus.Should().Be(BillingStatus.Current);

        // Snapshot integrity
        subscription.PlanCodeSnapshot.Should().Be("pro");
        subscription.MonthlyPriceSnapshot.Should().Be(149.00m);
        subscription.AnnualPriceSnapshot.Should().Be(1490.00m);
        subscription.CurrencySnapshot.Should().Be("USD");
        subscription.MaxDoctorsSnapshot.Should().Be(15);
        subscription.MaxUsersSnapshot.Should().Be(25);
        subscription.MaxPatientsSnapshot.Should().Be(5000);
        subscription.MaxStorageBytesSnapshot.Should().Be(50L * 1024 * 1024 * 1024);
        subscription.MonthlyAiRequestsLimitSnapshot.Should().Be(1500);
        subscription.MaxDicomStudiesMonthlySnapshot.Should().Be(100);
        subscription.FeaturesSnapshot.Should().Be("[\"ai_copilot\",\"dicom_pacs\"]");

        // Mutating the plan catalog MUST NOT alter the existing subscription snapshot
        plan.MonthlyPrice = 199.00m;
        plan.MaxDoctors = 20;
        plan.FeaturesJson = "[\"ai_copilot\"]";

        subscription.MonthlyPriceSnapshot.Should().Be(149.00m);
        subscription.MaxDoctorsSnapshot.Should().Be(15);
        subscription.FeaturesSnapshot.Should().Be("[\"ai_copilot\",\"dicom_pacs\"]");
    }

    [Fact]
    public void UsageMetric_ShouldCalculateQuotaAndPercentageAccurately()
    {
        var metric = new UsageMetric
        {
            ClinicId = Guid.NewGuid(),
            MetricType = MetricType.AiRequestsCount,
            PeriodStartUtc = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            PeriodEndUtc = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            QuotaLimit = 1000,
            CurrentValue = 500
        };

        metric.HasExceededQuota.Should().BeFalse();
        metric.UsagePercentage.Should().Be(50.0);

        metric.CurrentValue = 1000;
        metric.HasExceededQuota.Should().BeTrue();
        metric.UsagePercentage.Should().Be(100.0);

        metric.CurrentValue = 1200;
        metric.HasExceededQuota.Should().BeTrue();
        metric.UsagePercentage.Should().Be(100.0); // Capped at 100%
    }

    [Fact]
    public void UsageMetric_WithUnlimitedQuota_ShouldNeverExceedQuota()
    {
        var metric = new UsageMetric
        {
            ClinicId = Guid.NewGuid(),
            MetricType = MetricType.AiRequestsCount,
            QuotaLimit = 0, // 0 = unlimited
            CurrentValue = 999999
        };

        metric.HasExceededQuota.Should().BeFalse();
        metric.UsagePercentage.Should().Be(0.0);
    }

    [Fact]
    public void FeatureKey_All_ShouldContainExpectedConstantsAndBeUnique()
    {
        FeatureKey.All.Should().Contain(
        [
            FeatureKey.AiCopilot,
            FeatureKey.DicomPacs,
            FeatureKey.Dental,
            FeatureKey.ExternalLabs,
            FeatureKey.Insurance,
            FeatureKey.PatientPortal,
            FeatureKey.AdvancedReports,
            FeatureKey.CustomBranding,
            FeatureKey.ApiAccess
        ]);

        FeatureKey.All.Should().OnlyHaveUniqueItems();
    }
}
