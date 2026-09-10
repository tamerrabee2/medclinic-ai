using FluentAssertions;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Services;
using MedClinic.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace MedClinic.Tests.Integration;

public class PostgreSqlQuotaDistributedConcurrencyTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private PostgreSqlContainer? _postgresContainer;
    private bool _dockerAvailable;
    private string? _initializationError;

    private static bool IsContinuousIntegration =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD"));

    public PostgreSqlQuotaDistributedConcurrencyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("medclinic_test")
                .WithUsername("testuser")
                .WithPassword("testpass123")
                .Build();

            await _postgresContainer.StartAsync();
            _dockerAvailable = true;
            _output.WriteLine("PostgreSQL Testcontainer successfully started.");
        }
        catch (Exception ex)
        {
            _dockerAvailable = false;
            _initializationError = ex.Message;
            _output.WriteLine($"Docker / PostgreSQL container unavailable on this host: {ex.Message}.");
        }
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    private DbContextOptions<ApplicationDbContext> CreatePostgreSqlOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgresContainer!.GetConnectionString())
            .Options;
    }

    [Fact]
    public void CiEnvironment_RequiresDockerAndTestcontainers()
    {
        if (IsContinuousIntegration)
        {
            _dockerAvailable.Should().BeTrue("Continuous Integration (CI) requires Docker and PostgreSQL Testcontainers to guarantee distributed concurrency enforcement.");
        }
    }

    [Fact]
    public async Task ReserveQuotaAsync_DistributedAcrossMultipleIsolatedNodes_NeverExceedsPostgresQuota()
    {
        if (!_dockerAvailable)
        {
            if (IsContinuousIntegration)
            {
                Assert.Fail($"Docker/Testcontainers is required in CI to execute distributed PostgreSQL concurrency tests, but could not be started: {_initializationError}");
            }
            _output.WriteLine($"SKIPPED_LOCAL: Docker daemon is not running locally ({_initializationError}). Test is enforced in CI.");
            return;
        }

        var options = CreatePostgreSqlOptions();
        using (var initDb = new ApplicationDbContext(options))
        {
            await initDb.Database.EnsureCreatedAsync();
        }

        var clinicId = Guid.NewGuid();

        // Seed Clinic and Pro subscription with limit = 5 in PostgreSQL
        using (var seedDb = new ApplicationDbContext(options))
        {
            seedDb.Clinics.Add(new Clinic
            {
                Id = clinicId,
                Name = "Distributed Al-Shifa Clinic",
                Slug = $"dist-{Guid.NewGuid():N}"[..10],
                LifecycleStatus = ClinicLifecycleStatus.Active,
                BillingStatus = BillingStatus.Current
            });

            seedDb.ClinicSubscriptions.Add(new ClinicSubscription
            {
                ClinicId = clinicId,
                Tier = SubscriptionTier.Pro,
                Status = SubscriptionStatus.Active,
                IsActive = true,
                StartDateUtc = DateTime.UtcNow.AddDays(-5),
                MonthlyAiRequestsLimitSnapshot = 5,
                FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
            });

            await seedDb.SaveChangesAsync();
        }

        // Spawn 20 parallel requests simulating 20 distinct distributed web application nodes.
        // Each node has its own separate DbContext AND its own separate KeyedLockManager (no shared memory).
        const int concurrentRequests = 20;
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async i =>
        {
            await Task.Yield();
            using var nodeDb = new ApplicationDbContext(options);
            var isolatedNodeLockManager = new KeyedLockManager();
            var nodeService = new TenantEntitlementService(nodeDb, isolatedNodeLockManager);

            return await nodeService.ReserveQuotaAsync(
                clinicId: clinicId,
                metricType: MetricType.AiRequestsCount,
                delta: 1,
                idempotencyKey: $"dist-req-{i:D3}",
                operationId: $"node-op-{i}");
        }).ToList();

        var results = await Task.WhenAll(tasks);

        int successCount = results.Count(r => r.IsAllowed);
        int rejectedCount = results.Count(r => !r.IsAllowed && r.Code == "quota_exceeded");

        _output.WriteLine($"Distributed execution results: Successes = {successCount}, Rejections = {rejectedCount}");

        successCount.Should().Be(5, "PostgreSQL advisory locking must restrict concurrent reservations strictly to quota limit");
        rejectedCount.Should().Be(15, "All requests exceeding quota limit under concurrency must be rejected");

        // Verify database state in PostgreSQL
        using (var verifyDb = new ApplicationDbContext(options))
        {
            var totalReservedInDb = await verifyDb.UsageEvents
                .Where(e => e.ClinicId == clinicId && e.Status == UsageEventStatus.Reserved)
                .SumAsync(e => e.Delta);

            totalReservedInDb.Should().Be(5, "PostgreSQL database must strictly record 5 reserved units, never 6+");
        }
    }

    [Fact]
    public async Task CommitReservationAsync_UnderConcurrentRetriesAcrossDistributedNodes_IncrementsMetricExactlyOnce()
    {
        if (!_dockerAvailable)
        {
            if (IsContinuousIntegration)
            {
                Assert.Fail($"Docker/Testcontainers is required in CI to execute distributed PostgreSQL concurrency tests, but could not be started: {_initializationError}");
            }
            _output.WriteLine($"SKIPPED_LOCAL: Docker daemon is not running locally ({_initializationError}). Test is enforced in CI.");
            return;
        }

        var options = CreatePostgreSqlOptions();
        using (var initDb = new ApplicationDbContext(options))
        {
            await initDb.Database.EnsureCreatedAsync();
        }

        var clinicId = Guid.NewGuid();

        // Seed Clinic and subscription
        using (var seedDb = new ApplicationDbContext(options))
        {
            seedDb.Clinics.Add(new Clinic
            {
                Id = clinicId,
                Name = "Commit Idempotency Clinic",
                Slug = $"cmt-{Guid.NewGuid():N}"[..10],
                LifecycleStatus = ClinicLifecycleStatus.Active,
                BillingStatus = BillingStatus.Current
            });

            seedDb.ClinicSubscriptions.Add(new ClinicSubscription
            {
                ClinicId = clinicId,
                Tier = SubscriptionTier.Pro,
                Status = SubscriptionStatus.Active,
                IsActive = true,
                StartDateUtc = DateTime.UtcNow.AddDays(-5),
                MonthlyAiRequestsLimitSnapshot = 100,
                FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot })
            });

            await seedDb.SaveChangesAsync();
        }

        // Create 1 reservation with delta = 1
        Guid reservationId;
        using (var reserveDb = new ApplicationDbContext(options))
        {
            var entitlement = new TenantEntitlementService(reserveDb);
            var res = await entitlement.ReserveQuotaAsync(
                clinicId: clinicId,
                metricType: MetricType.AiRequestsCount,
                delta: 1,
                idempotencyKey: "dist-commit-key-001",
                operationId: "initial-op-001");

            res.IsAllowed.Should().BeTrue();
            reservationId = res.ReservationId!.Value;
        }

        // 10 concurrent commit retry requests across 10 independent nodes (separate DbContexts, separate lock managers)
        const int concurrentCommits = 10;
        var commitTasks = Enumerable.Range(0, concurrentCommits).Select(async _ =>
        {
            await Task.Yield();
            using var nodeDb = new ApplicationDbContext(options);
            var isolatedNodeLock = new KeyedLockManager();
            var nodeEntitlement = new TenantEntitlementService(nodeDb, isolatedNodeLock);

            await nodeEntitlement.CommitReservationAsync(reservationId);
        });

        await Task.WhenAll(commitTasks);

        // Verify in PostgreSQL that UsageMetric was incremented EXACTLY ONCE (value = 1, NOT 10)
        using (var verifyDb = new ApplicationDbContext(options))
        {
            var metric = await verifyDb.UsageMetrics
                .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.AiRequestsCount);

            metric.Should().NotBeNull();
            metric!.CurrentValue.Should().Be(1, "Concurrent commit retries across distributed nodes must increment UsageMetric exactly once");

            var reservationEvent = await verifyDb.UsageEvents.FirstAsync(e => e.Id == reservationId);
            reservationEvent.Status.Should().Be(UsageEventStatus.Committed);
        }
    }
}
