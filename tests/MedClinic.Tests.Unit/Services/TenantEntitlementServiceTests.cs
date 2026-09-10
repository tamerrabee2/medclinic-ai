using FluentAssertions;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Services;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Unit.Helpers;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class TenantEntitlementServiceTests
{
    [Fact]
    public async Task CheckCanExecuteAsync_WhenClinicIsActive_AllowsAllActions()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Active Health Center",
            Slug = "active-health",
            LifecycleStatus = ClinicLifecycleStatus.Active,
            BillingStatus = BillingStatus.Current
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        var resultAppointment = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.CreateAppointment);
        var resultVisit = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.CreateVisit);
        var resultAi = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.UseAiCopilot);
        var resultView = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.ViewRecords);

        resultAppointment.IsAllowed.Should().BeTrue();
        resultVisit.IsAllowed.Should().BeTrue();
        resultAi.IsAllowed.Should().BeTrue();
        resultView.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task CheckCanExecuteAsync_WhenClinicIsSuspended_EnforcesAccessMatrix()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Suspended Medical Center",
            Slug = "suspended-medical",
            LifecycleStatus = ClinicLifecycleStatus.Suspended,
            SuspensionReason = "Payment dispute pending review"
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        // 1. Preserved Under Suspension (Read-Only, Compliance, Patient Rights)
        var viewResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.ViewRecords);
        var auditResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.AccessAuditLogs);
        var consentRevokeResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.RevokeConsent);
        var legalHoldResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.ManageLegalHold);
        var loginResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.Login);

        viewResult.IsAllowed.Should().BeTrue();
        auditResult.IsAllowed.Should().BeTrue();
        consentRevokeResult.IsAllowed.Should().BeTrue();
        legalHoldResult.IsAllowed.Should().BeTrue();
        loginResult.IsAllowed.Should().BeTrue();

        // 2. Blocked Under Suspension (Operational Mutations & Consumptions)
        var apptResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.CreateAppointment);
        var visitResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.CreateVisit);
        var rxResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.CreatePrescription);
        var aiResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.UseAiCopilot);
        var dicomResult = await sut.CheckCanExecuteAsync(clinicId, ClinicalAction.UploadDicom);

        apptResult.IsAllowed.Should().BeFalse();
        apptResult.Code.Should().Be("clinic_suspended");
        apptResult.Reason.Should().Contain("Payment dispute");

        visitResult.IsAllowed.Should().BeFalse();
        rxResult.IsAllowed.Should().BeFalse();
        aiResult.IsAllowed.Should().BeFalse();
        dicomResult.IsAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task HasFeatureAsync_EvaluatesSubscriptionFeatureSnapshot()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = "pro",
            Name = "Pro",
            Tier = SubscriptionTier.Pro,
            MonthlyPrice = 149m,
            AnnualPrice = 1490m,
            FeaturesJson = "[\"ai_copilot\",\"dicom_pacs\"]"
        };
        db.SubscriptionPlans.Add(plan);

        var subscription = ClinicSubscription.CreateWithSnapshot(
            clinicId,
            plan,
            BillingCycle.Monthly,
            SubscriptionStatus.Active);
        db.ClinicSubscriptions.Add(subscription);
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        var hasAi = await sut.HasFeatureAsync(clinicId, FeatureKey.AiCopilot);
        var hasDicom = await sut.HasFeatureAsync(clinicId, FeatureKey.DicomPacs);
        var hasDental = await sut.HasFeatureAsync(clinicId, FeatureKey.Dental);

        hasAi.Should().BeTrue();
        hasDicom.Should().BeTrue();
        hasDental.Should().BeFalse();
    }

    [Fact]
    public async Task RecordUsageAsync_And_CheckQuotaAsync_HandlesMonthlyBuckets()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = "basic",
            Name = "Basic",
            Tier = SubscriptionTier.Basic,
            MonthlyPrice = 49m,
            AnnualPrice = 490m,
            MonthlyAiRequestsLimit = 5,
            FeaturesJson = "[]"
        };
        db.SubscriptionPlans.Add(plan);

        var subscription = ClinicSubscription.CreateWithSnapshot(
            clinicId,
            plan,
            BillingCycle.Monthly,
            SubscriptionStatus.Active);
        db.ClinicSubscriptions.Add(subscription);
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        // Initially under quota
        var initialQuota = await sut.CheckQuotaAsync(clinicId, MetricType.AiRequestsCount);
        initialQuota.IsAllowed.Should().BeTrue();

        // Increment usage by 3
        await sut.RecordUsageAsync(clinicId, MetricType.AiRequestsCount, 3);
        var midQuota = await sut.CheckQuotaAsync(clinicId, MetricType.AiRequestsCount);
        midQuota.IsAllowed.Should().BeTrue();

        // Increment usage by 2 more (now 5 == limit)
        await sut.RecordUsageAsync(clinicId, MetricType.AiRequestsCount, 2);
        var fullQuota = await sut.CheckQuotaAsync(clinicId, MetricType.AiRequestsCount);
        fullQuota.IsAllowed.Should().BeFalse();
        fullQuota.Code.Should().Be("quota_exceeded");
        fullQuota.CurrentValue.Should().Be(5);
        fullQuota.QuotaLimit.Should().Be(5);
    }

    [Fact]
    public async Task GetSubscriptionSummaryAsync_ReturnsAccurateQuotasAndStatuses()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        var clinic = new Clinic
        {
            Id = clinicId,
            Name = "Al-Shifa Clinic",
            Slug = "al-shifa",
            LifecycleStatus = ClinicLifecycleStatus.Active,
            BillingStatus = BillingStatus.Current
        };
        db.Clinics.Add(clinic);

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = "pro",
            Name = "Pro Clinic Plan",
            Tier = SubscriptionTier.Pro,
            MonthlyPrice = 149m,
            AnnualPrice = 1490m,
            MaxDoctors = 10,
            MaxUsers = 20,
            MaxPatients = 2000,
            MonthlyAiRequestsLimit = 500,
            FeaturesJson = "[\"ai_copilot\"]"
        };
        db.SubscriptionPlans.Add(plan);

        var subscription = ClinicSubscription.CreateWithSnapshot(
            clinicId,
            plan,
            BillingCycle.Monthly,
            SubscriptionStatus.Active);
        db.ClinicSubscriptions.Add(subscription);

        // Add 2 doctors
        db.Doctors.Add(new Doctor { Id = Guid.NewGuid(), ClinicId = clinicId, FirstName = "Dr. Zaid", LastName = "Ali", Email = "zaid@test.com", CreatedAt = DateTime.UtcNow });
        db.Doctors.Add(new Doctor { Id = Guid.NewGuid(), ClinicId = clinicId, FirstName = "Dr. Sarah", LastName = "Omar", Email = "sarah@test.com", CreatedAt = DateTime.UtcNow });

        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);
        var summary = await sut.GetSubscriptionSummaryAsync(clinicId);

        summary.ClinicId.Should().Be(clinicId);
        summary.ClinicName.Should().Be("Al-Shifa Clinic");
        summary.PlanCode.Should().Be("pro");
        summary.Tier.Should().Be(SubscriptionTier.Pro);
        summary.EnabledFeatures.Should().Contain(FeatureKey.AiCopilot);

        var doctorQuota = summary.Quotas.First(q => q.MetricType == MetricType.DoctorsCount);
        doctorQuota.CurrentValue.Should().Be(2);
        doctorQuota.QuotaLimit.Should().Be(10);
        doctorQuota.UsagePercentage.Should().Be(20.0);
        doctorQuota.IsExceeded.Should().BeFalse();
    }
}
