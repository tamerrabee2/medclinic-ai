using FluentAssertions;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Services;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class AtomicQuotaReservationTests
{
    [Fact]
    public async Task ReserveQuotaAsync_WhenQuotaAvailable_CreatesReservedUsageEvent()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Al-Shifa Clinic",
            Slug = "al-shifa",
            LifecycleStatus = ClinicLifecycleStatus.Active,
            BillingStatus = BillingStatus.Current
        });

        // Pro tier with 500 AI requests
        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            StartDateUtc = DateTime.UtcNow.AddDays(-10),
            MonthlyAiRequestsLimitSnapshot = 500,
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);
        var idempotencyKey = "req-ai-001";

        var result = await sut.ReserveQuotaAsync(
            clinicId,
            MetricType.AiRequestsCount,
            delta: 1,
            idempotencyKey: idempotencyKey,
            operationId: "ai-inference-001");

        result.IsAllowed.Should().BeTrue();
        result.ReservationId.Should().NotBeNull();
        result.MetricType.Should().Be(MetricType.AiRequestsCount);

        var savedEvent = await db.UsageEvents.FirstOrDefaultAsync(e => e.Id == result.ReservationId!.Value);
        savedEvent.Should().NotBeNull();
        savedEvent!.Status.Should().Be(UsageEventStatus.Reserved);
        savedEvent.ClinicId.Should().Be(clinicId);
        savedEvent.Delta.Should().Be(1);
        savedEvent.IdempotencyKey.Should().Be(idempotencyKey);
    }

    [Fact]
    public async Task ReserveQuotaAsync_PreventsConcurrentOverrun_WhenActiveReservationsReachLimit()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Peak AI Hospital",
            Slug = "peak-ai",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });

        // Limit is strictly 2 AI requests
        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            MonthlyAiRequestsLimitSnapshot = 2,
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        // First reservation (1 of 2)
        var res1 = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "key-1", "op-1");
        res1.IsAllowed.Should().BeTrue();

        // Second reservation (2 of 2)
        var res2 = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "key-2", "op-2");
        res2.IsAllowed.Should().BeTrue();

        // Third concurrent reservation must be rejected atomically even before op1 or op2 complete
        var res3 = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "key-3", "op-3");
        res3.IsAllowed.Should().BeFalse();
        res3.Code.Should().Be("quota_exceeded");
    }

    [Fact]
    public async Task ReserveQuotaAsync_WithSameIdempotencyKey_ReturnsExistingReservationWithoutDoubleCounting()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Idempotent Clinic",
            Slug = "idempotent-clinic",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });

        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            MonthlyAiRequestsLimitSnapshot = 10,
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);
        var key = "idempotent-key-xyz";

        var res1 = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, key, "op-xyz");
        res1.IsAllowed.Should().BeTrue();

        // Repeat with identical key
        var res2 = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, key, "op-xyz-retry");
        res2.IsAllowed.Should().BeTrue();
        res2.ReservationId.Should().Be(res1.ReservationId);

        var count = await db.UsageEvents.CountAsync(e => e.ClinicId == clinicId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task CommitReservationAsync_UpdatesEventStatusAndIncrementsUsageMetric()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Committed Clinic",
            Slug = "committed-clinic",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });

        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            MonthlyAiRequestsLimitSnapshot = 100,
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        var reservation = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "commit-key-1", "commit-op-1");
        reservation.IsAllowed.Should().BeTrue();

        await sut.CommitReservationAsync(reservation.ReservationId!.Value);

        var evt = await db.UsageEvents.FindAsync(reservation.ReservationId!.Value);
        evt.Should().NotBeNull();
        evt!.Status.Should().Be(UsageEventStatus.Committed);
        evt.CommittedAtUtc.Should().NotBeNull();

        var metric = await db.UsageMetrics.FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.AiRequestsCount);
        metric.Should().NotBeNull();
        metric!.CurrentValue.Should().Be(1);
    }

    [Fact]
    public async Task ReleaseReservationAsync_FreesReservedQuotaForSubsequentRequests()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Release Clinic",
            Slug = "release-clinic",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });

        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            MonthlyAiRequestsLimitSnapshot = 1, // Only 1 allowed
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        // Reserve the single slot
        var res1 = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "res-slot-1", "op-slot-1");
        res1.IsAllowed.Should().BeTrue();

        // Second slot is blocked
        var resBlocked = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "res-slot-2", "op-slot-2");
        resBlocked.IsAllowed.Should().BeFalse();

        // Release first reservation due to operation failure
        await sut.ReleaseReservationAsync(res1.ReservationId!.Value, "Simulated LLM network timeout");

        var evt = await db.UsageEvents.FindAsync(res1.ReservationId!.Value);
        evt!.Status.Should().Be(UsageEventStatus.Released);
        evt.ReleaseReason.Should().Be("Simulated LLM network timeout");

        // Now retry or new request should succeed!
        var resRetry = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "res-slot-2-retry", "op-slot-2-retry");
        resRetry.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task ReserveQuotaAsync_WhenClinicIsSuspended_BlocksReservation()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Suspended Lab",
            Slug = "suspended-lab",
            LifecycleStatus = ClinicLifecycleStatus.Suspended,
            SuspensionReason = "Account overdue"
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        var result = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, "key-susp", "op-susp");
        result.IsAllowed.Should().BeFalse();
        result.Code.Should().Be("clinic_suspended");
    }
}
