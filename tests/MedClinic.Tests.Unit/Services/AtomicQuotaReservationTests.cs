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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ReserveQuotaAsync_WhenDeltaIsZeroOrNegative_ThrowsArgumentOutOfRangeException(long invalidDelta)
    {
        using var db = TestDbContextFactory.Create();
        var sut = new TenantEntitlementService(db);

        var act = () => sut.ReserveQuotaAsync(
            Guid.NewGuid(),
            MetricType.AiRequestsCount,
            delta: invalidDelta,
            idempotencyKey: "key-inv",
            operationId: "op-inv");

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("delta");
    }

    [Fact]
    public async Task CommitReservationAsync_WhenReservationExpired_MarksExpiredAndThrowsQuotaReservationExpiredException()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Expiring Clinic",
            Slug = "expiring-clinic",
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

        // Add an already-expired reservation
        var expiredReservation = new UsageEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            MetricType = MetricType.AiRequestsCount,
            PeriodStartUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            PeriodEndUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1),
            OperationId = "op-expired",
            IdempotencyKey = "key-expired",
            Delta = 1,
            Status = UsageEventStatus.Reserved,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5), // Expired 5 minutes ago!
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        db.UsageEvents.Add(expiredReservation);
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        var act = () => sut.CommitReservationAsync(expiredReservation.Id);

        await act.Should().ThrowAsync<MedClinic.Domain.Exceptions.QuotaReservationExpiredException>();

        var updated = await db.UsageEvents.FindAsync(expiredReservation.Id);
        updated!.Status.Should().Be(UsageEventStatus.Expired);
        updated.ReleaseReason.Should().Contain("expired");
    }

    [Fact]
    public async Task ReserveQuotaAsync_WithSameIdempotencyKeyAcrossDifferentMetrics_AllowsBothIndependently()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Multi-Metric Clinic",
            Slug = "multi-metric",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });

        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Enterprise,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            MonthlyAiRequestsLimitSnapshot = 100,
            MaxDicomStudiesMonthlySnapshot = 50,
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot, FeatureKey.DicomPacs })
        });
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);
        const string sharedKey = "shared-idempotency-key-001";

        // Same idempotency key for AI
        var resAi = await sut.ReserveQuotaAsync(clinicId, MetricType.AiRequestsCount, 1, sharedKey, "op-ai");
        resAi.IsAllowed.Should().BeTrue();

        // Same idempotency key for DICOM
        var resDicom = await sut.ReserveQuotaAsync(clinicId, MetricType.DicomStudiesCount, 1, sharedKey, "op-dicom");
        resDicom.IsAllowed.Should().BeTrue();
        resDicom.ReservationId.Should().NotBeNull();
        resDicom.ReservationId!.Value.Should().NotBe(resAi.ReservationId!.Value);
    }

    [Fact]
    public async Task CleanupExpiredReservationsAsync_MarksStaleReservationsAsExpired()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // 2 expired reservations + 1 active reservation
        db.UsageEvents.AddRange(
            new UsageEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                MetricType = MetricType.AiRequestsCount,
                PeriodStartUtc = periodStart,
                PeriodEndUtc = periodStart.AddMonths(1),
                OperationId = "stale-1",
                IdempotencyKey = "key-stale-1",
                Delta = 1,
                Status = UsageEventStatus.Reserved,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-10),
                CreatedAt = DateTime.UtcNow.AddMinutes(-15)
            },
            new UsageEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                MetricType = MetricType.AiRequestsCount,
                PeriodStartUtc = periodStart,
                PeriodEndUtc = periodStart.AddMonths(1),
                OperationId = "stale-2",
                IdempotencyKey = "key-stale-2",
                Delta = 1,
                Status = UsageEventStatus.Reserved,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-2),
                CreatedAt = DateTime.UtcNow.AddMinutes(-7)
            },
            new UsageEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                MetricType = MetricType.AiRequestsCount,
                PeriodStartUtc = periodStart,
                PeriodEndUtc = periodStart.AddMonths(1),
                OperationId = "fresh-3",
                IdempotencyKey = "key-fresh-3",
                Delta = 1,
                Status = UsageEventStatus.Reserved,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5), // Still valid!
                CreatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var sut = new TenantEntitlementService(db);

        var cleaned = await sut.CleanupExpiredReservationsAsync();
        cleaned.Should().Be(2);

        var events = await db.UsageEvents.ToListAsync();
        events.Count(e => e.Status == UsageEventStatus.Expired).Should().Be(2);
        events.Count(e => e.Status == UsageEventStatus.Reserved).Should().Be(1);
    }

    [Fact]
    public async Task ReserveQuotaAsync_UnderTrueMultiThreadedConcurrency_NeverExceedsLimit()
    {
        var dbName = Guid.NewGuid().ToString();
        var clinicId = Guid.NewGuid();

        // 1. Seed database using a dedicated DbContext
        using (var seedDb = TestDbContextFactory.Create(dbName))
        {
            seedDb.Clinics.Add(new Clinic
            {
                Id = clinicId,
                Name = "High Concurrency Medical Center",
                Slug = "high-concurrency",
                LifecycleStatus = ClinicLifecycleStatus.Active,
                BillingStatus = BillingStatus.Current
            });

            // Limit is strictly 5
            seedDb.ClinicSubscriptions.Add(new ClinicSubscription
            {
                ClinicId = clinicId,
                Tier = SubscriptionTier.Pro,
                Status = SubscriptionStatus.Active,
                IsActive = true,
                MonthlyAiRequestsLimitSnapshot = 5,
                FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
            });
            await seedDb.SaveChangesAsync();
        }

        // 2. Shared KeyedLockManager across instances
        var keyedLock = new KeyedLockManager();
        const int totalRequests = 20;
        const int allowedLimit = 5;

        // 3. Fire 20 parallel requests concurrently, each using its own DbContext instance and unique idempotency key
        var tasks = Enumerable.Range(1, totalRequests).Select(async i =>
        {
            using var db = TestDbContextFactory.Create(dbName);
            var sut = new TenantEntitlementService(db, keyedLock);

            return await sut.ReserveQuotaAsync(
                clinicId,
                MetricType.AiRequestsCount,
                delta: 1,
                idempotencyKey: $"concurrent-key-{i}",
                operationId: $"concurrent-op-{i}");
        }).ToArray();

        var results = await Task.WhenAll(tasks);

        // 4. Assert: Exactly 5 succeed and 15 fail with quota_exceeded
        var successCount = results.Count(r => r.IsAllowed);
        var rejectedCount = results.Count(r => !r.IsAllowed && r.Code == "quota_exceeded");

        successCount.Should().Be(allowedLimit, "Only exactly the quota limit of 5 requests should be granted reservations");
        rejectedCount.Should().Be(totalRequests - allowedLimit, "All excess 15 concurrent requests must be rejected atomically");

        // 5. Query final state from a fresh DbContext
        using (var verifyDb = TestDbContextFactory.Create(dbName))
        {
            var totalReserved = await verifyDb.UsageEvents
                .Where(e => e.ClinicId == clinicId && e.Status == UsageEventStatus.Reserved)
                .SumAsync(e => e.Delta);

            totalReserved.Should().Be(allowedLimit, "Total reserved quota in database must strictly equal limit and never exceed it under race conditions");
        }
    }
}
