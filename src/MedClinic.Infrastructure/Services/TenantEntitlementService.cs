using System.Text.Json;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Features.Subscriptions.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Domain.Exceptions;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Services;

public sealed class TenantEntitlementService : ITenantEntitlementService
{
    private readonly ApplicationDbContext _db;
    private readonly IKeyedLockManager _keyedLock;
    private static readonly IKeyedLockManager _defaultLock = new KeyedLockManager();

    public TenantEntitlementService(ApplicationDbContext db, IKeyedLockManager? keyedLock = null)
    {
        _db = db;
        _keyedLock = keyedLock ?? _defaultLock;
    }

    public async Task<bool> HasFeatureAsync(Guid clinicId, string featureKey, CancellationToken ct = default)
    {
        var features = await GetActiveFeaturesAsync(clinicId, ct);
        return features.Contains(featureKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetActiveFeaturesAsync(Guid clinicId, CancellationToken ct = default)
    {
        // 1. Check active subscription snapshot first
        var subscription = await _db.ClinicSubscriptions
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.IsActive && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefaultAsync(ct);

        if (subscription is not null && !string.IsNullOrWhiteSpace(subscription.FeaturesSnapshot))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(subscription.FeaturesSnapshot);
                return list ?? [];
            }
            catch
            {
                // Fallback on corrupt JSON
            }
        }

        // 2. Check clinic's plan fallback
        var clinic = await _db.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct);

        if (clinic is null)
            return [];

        // Trial clinics receive full trial access to all Pro features
        if (clinic.LifecycleStatus == ClinicLifecycleStatus.Trial)
        {
            return
            [
                FeatureKey.AiCopilot,
                FeatureKey.DicomPacs,
                FeatureKey.ExternalLabs,
                FeatureKey.PatientPortal,
                FeatureKey.AdvancedReports
            ];
        }

        // Legacy clinic plan mapping fallback
        var planCode = clinic.Plan.ToString().ToLowerInvariant();
        var defaultPlan = await _db.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == planCode || p.Code == "basic", ct);

        if (defaultPlan is not null && !string.IsNullOrWhiteSpace(defaultPlan.FeaturesJson))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(defaultPlan.FeaturesJson);
                return list ?? [];
            }
            catch
            {
                // Fallback
            }
        }

        return [];
    }

    public async Task<EntitlementResult> CheckCanExecuteAsync(Guid clinicId, ClinicalAction action, CancellationToken ct = default)
    {
        var clinic = await _db.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct);

        if (clinic is null)
            return EntitlementResult.Suspended("Clinic not found.");

        // Tenant Suspension Access Matrix:
        // Preserved under suspension: Read-only EMR, audit trail, consent revocation, legal hold, restricted login.
        bool isPreservedAction = action is ClinicalAction.ViewRecords
            or ClinicalAction.AccessAuditLogs
            or ClinicalAction.RevokeConsent
            or ClinicalAction.ManageLegalHold
            or ClinicalAction.Login;

        if (clinic.LifecycleStatus is ClinicLifecycleStatus.Suspended or ClinicLifecycleStatus.Cancelled)
        {
            if (isPreservedAction)
                return EntitlementResult.Success();

            return EntitlementResult.Suspended(clinic.SuspensionReason ?? "Clinic operations are suspended. Read-only compliance access is permitted.");
        }

        // Check Past Due Billing
        if (clinic.BillingStatus is BillingStatus.PastDue or BillingStatus.GracePeriod)
        {
            return EntitlementResult.PastDueGracePeriod();
        }

        if (clinic.BillingStatus is BillingStatus.Failed)
        {
            if (isPreservedAction)
                return EntitlementResult.Success();

            return EntitlementResult.Suspended("Clinic billing has failed. Operational mutations are suspended until billing is resolved.");
        }

        return EntitlementResult.Success();
    }

    public async Task<EntitlementResult> CheckQuotaAsync(Guid clinicId, MetricType metricType, CancellationToken ct = default)
    {
        var subscription = await _db.ClinicSubscriptions
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.IsActive && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefaultAsync(ct);

        var clinic = subscription is null
            ? await _db.Clinics.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            : null;
        bool isTrial = clinic?.LifecycleStatus == ClinicLifecycleStatus.Trial;

        long limit = 0;
        long currentValue = 0;

        switch (metricType)
        {
            case MetricType.DoctorsCount:
                limit = subscription?.MaxDoctorsSnapshot ?? (isTrial ? 15 : 3);
                if (limit == 0) return EntitlementResult.Success(); // 0 = unlimited
                currentValue = await _db.Doctors.CountAsync(d => d.ClinicId == clinicId && !d.IsDeleted, ct);
                break;

            case MetricType.UsersCount:
                limit = subscription?.MaxUsersSnapshot ?? (isTrial ? 25 : 5);
                if (limit == 0) return EntitlementResult.Success();
                currentValue = await _db.ClinicMembers.CountAsync(m => m.ClinicId == clinicId && !m.IsDeleted, ct);
                break;

            case MetricType.PatientsCount:
                limit = subscription?.MaxPatientsSnapshot ?? (isTrial ? 5000 : 500);
                if (limit == 0) return EntitlementResult.Success();
                currentValue = await _db.Patients.CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted, ct);
                break;

            case MetricType.AiRequestsCount:
                limit = subscription?.MonthlyAiRequestsLimitSnapshot ?? (isTrial ? 1500 : 100);
                if (limit == 0) return EntitlementResult.Success();
                var (aiStart, _) = GetCurrentMonthPeriod();
                var aiMetric = await _db.UsageMetrics
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.AiRequestsCount && m.PeriodStartUtc == aiStart, ct);
                currentValue = aiMetric?.CurrentValue ?? 0;
                break;

            case MetricType.DicomStudiesCount:
                limit = subscription?.MaxDicomStudiesMonthlySnapshot ?? 0;
                if (limit == 0)
                {
                    // 0 on snapshot for basic plan means feature not supported or 0 allowed
                    if (subscription?.Tier == SubscriptionTier.Basic)
                        return EntitlementResult.QuotaExceeded(metricType, 0, 0, "DICOM PACS studies are not supported on Basic tier.");
                    return EntitlementResult.Success();
                }
                var (dicomStart, _) = GetCurrentMonthPeriod();
                var dicomMetric = await _db.UsageMetrics
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.DicomStudiesCount && m.PeriodStartUtc == dicomStart, ct);
                currentValue = dicomMetric?.CurrentValue ?? 0;
                break;

            case MetricType.StorageBytes:
                limit = subscription?.MaxStorageBytesSnapshot ?? 5L * 1024 * 1024 * 1024;
                if (limit == 0) return EntitlementResult.Success();
                var (storageStart, _) = GetCurrentMonthPeriod();
                var storageMetric = await _db.UsageMetrics
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.StorageBytes && m.PeriodStartUtc == storageStart, ct);
                currentValue = storageMetric?.CurrentValue ?? 0;
                break;
        }

        if (limit > 0 && currentValue >= limit)
        {
            return EntitlementResult.QuotaExceeded(metricType, currentValue, limit);
        }

        return EntitlementResult.Success();
    }

    public async Task RecordUsageAsync(Guid clinicId, MetricType metricType, long amount = 1, CancellationToken ct = default)
    {
        var (periodStart, periodEnd) = GetCurrentMonthPeriod();

        var metric = await _db.UsageMetrics
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == metricType && m.PeriodStartUtc == periodStart, ct);

        if (metric is null)
        {
            var subscription = await _db.ClinicSubscriptions
                .AsNoTracking()
                .Where(s => s.ClinicId == clinicId && s.IsActive && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartDateUtc)
                .FirstOrDefaultAsync(ct);

            long limit = metricType switch
            {
                MetricType.AiRequestsCount => subscription?.MonthlyAiRequestsLimitSnapshot ?? 100,
                MetricType.DicomStudiesCount => subscription?.MaxDicomStudiesMonthlySnapshot ?? 0,
                MetricType.StorageBytes => subscription?.MaxStorageBytesSnapshot ?? 5L * 1024 * 1024 * 1024,
                _ => 0
            };

            metric = new UsageMetric
            {
                ClinicId = clinicId,
                MetricType = metricType,
                PeriodStartUtc = periodStart,
                PeriodEndUtc = periodEnd,
                QuotaLimit = limit,
                CurrentValue = amount,
                LastUpdatedAtUtc = DateTime.UtcNow
            };

            _db.UsageMetrics.Add(metric);
        }
        else
        {
            metric.CurrentValue += amount;
            metric.LastUpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<QuotaReservationResult> ReserveQuotaAsync(
        Guid clinicId,
        MetricType metricType,
        long delta,
        string idempotencyKey,
        string operationId,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        // P1: Validate delta > 0
        if (delta <= 0)
            throw new ArgumentOutOfRangeException(nameof(delta), "Delta must be strictly positive (greater than 0).");

        var now = DateTime.UtcNow;
        var (periodStart, periodEnd) = GetCurrentMonthPeriod();

        // 1. Verify tenant suspension matrix
        var clinic = await _db.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct);

        if (clinic is null)
            return QuotaReservationResult.Suspended("Clinic not found.");

        if (clinic.LifecycleStatus is ClinicLifecycleStatus.Suspended or ClinicLifecycleStatus.Cancelled)
            return QuotaReservationResult.Suspended(clinic.SuspensionReason ?? "Clinic operations are suspended. Operational reservations cannot be issued.");

        // 2. Verify feature entitlement
        if (metricType == MetricType.AiRequestsCount)
        {
            var hasAi = await HasFeatureAsync(clinicId, FeatureKey.AiCopilot, ct);
            if (!hasAi)
                return QuotaReservationResult.FeatureNotIncluded(FeatureKey.AiCopilot, "AI Copilot feature is not included in the active subscription plan.");
        }
        else if (metricType == MetricType.DicomStudiesCount)
        {
            var hasDicom = await HasFeatureAsync(clinicId, FeatureKey.DicomPacs, ct);
            if (!hasDicom)
                return QuotaReservationResult.FeatureNotIncluded(FeatureKey.DicomPacs, "DICOM PACS feature is not included in the active subscription plan.");
        }

        // P0: Concurrency control: Acquire in-process keyed lock per (ClinicId, MetricType, PeriodStartUtc)
        var lockKey = $"quota:{clinicId}:{metricType}:{periodStart:yyyyMM}";
        using var releaser = await _keyedLock.AcquireLockAsync(lockKey, ct);

        // P0: Database transactional locking (PostgreSQL advisory lock when on Npgsql)
        var isNpgsql = _db.Database.IsNpgsql();
        await using var tx = isNpgsql ? await _db.Database.BeginTransactionAsync(ct) : null;

        if (isNpgsql)
        {
            long advisoryKey = ComputeAdvisoryKey(lockKey);
            await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({advisoryKey})", ct);
        }

        // 3. P2: Check idempotency scoped by (ClinicId, MetricType, IdempotencyKey)
        var existing = await _db.UsageEvents
            .FirstOrDefaultAsync(e => e.ClinicId == clinicId && e.MetricType == metricType && e.IdempotencyKey == idempotencyKey, ct);

        if (existing is not null)
        {
            if (existing.Status is UsageEventStatus.Reserved or UsageEventStatus.Committed)
            {
                if (tx != null)
                {
                    await tx.CommitAsync(ct);
                }
                return QuotaReservationResult.Success(
                    existing.Id,
                    existing.MetricType,
                    existing.Delta,
                    0,
                    0,
                    existing.ExpiresAtUtc);
            }
        }

        // 4. Determine quota limit from active subscription snapshot or trial fallback
        var subscription = await _db.ClinicSubscriptions
            .AsNoTracking()
            .Where(s => s.ClinicId == clinicId && s.IsActive && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefaultAsync(ct);

        bool isTrial = clinic.LifecycleStatus == ClinicLifecycleStatus.Trial;

        long limit = metricType switch
        {
            MetricType.AiRequestsCount => subscription?.MonthlyAiRequestsLimitSnapshot ?? (isTrial ? 1500 : 100),
            MetricType.DicomStudiesCount => subscription?.MaxDicomStudiesMonthlySnapshot ?? (isTrial ? 250 : 0),
            MetricType.StorageBytes => subscription?.MaxStorageBytesSnapshot ?? 5L * 1024 * 1024 * 1024,
            _ => 0
        };

        // 5. Query current committed usage
        var currentMetric = await _db.UsageMetrics
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == metricType && m.PeriodStartUtc == periodStart, ct);
        long currentUsage = currentMetric?.CurrentValue ?? 0;

        // 6. Query sum of active reserved events (not yet committed, not expired)
        long activeReserved = await _db.UsageEvents
            .Where(e => e.ClinicId == clinicId
                && e.MetricType == metricType
                && e.Status == UsageEventStatus.Reserved
                && e.PeriodStartUtc == periodStart
                && e.ExpiresAtUtc > now)
            .SumAsync(e => e.Delta, ct);

        if (limit > 0 && (currentUsage + activeReserved + delta) > limit)
        {
            if (tx != null)
            {
                await tx.RollbackAsync(ct);
            }
            return QuotaReservationResult.QuotaExceeded(metricType, currentUsage, activeReserved, limit);
        }

        // 7. Atomically insert reservation
        var expiresAtUtc = now.Add(ttl ?? TimeSpan.FromMinutes(5));
        var reservation = new UsageEvent
        {
            ClinicId = clinicId,
            MetricType = metricType,
            PeriodStartUtc = periodStart,
            PeriodEndUtc = periodEnd,
            OperationId = operationId,
            IdempotencyKey = idempotencyKey,
            Delta = delta,
            Status = UsageEventStatus.Reserved,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAt = now
        };

        _db.UsageEvents.Add(reservation);
        await _db.SaveChangesAsync(ct);
        if (tx != null)
        {
            await tx.CommitAsync(ct);
        }

        return QuotaReservationResult.Success(
            reservation.Id,
            metricType,
            currentUsage,
            activeReserved + delta,
            limit,
            expiresAtUtc);
    }

    public async Task CommitReservationAsync(Guid reservationId, CancellationToken ct = default)
    {
        var reservation = await _db.UsageEvents
            .FirstOrDefaultAsync(e => e.Id == reservationId, ct);

        if (reservation is null || reservation.Status == UsageEventStatus.Committed)
            return;

        var now = DateTime.UtcNow;

        // P1: Check if reservation expired before commit
        if (reservation.ExpiresAtUtc <= now || reservation.Status == UsageEventStatus.Expired)
        {
            reservation.Status = UsageEventStatus.Expired;
            reservation.ReleasedAtUtc = now;
            reservation.ReleaseReason = "Reservation expired before commit.";
            await _db.SaveChangesAsync(ct);

            throw new QuotaReservationExpiredException(
                reservationId,
                reservation.ExpiresAtUtc,
                $"Cannot commit reservation '{reservationId}' because it expired at {reservation.ExpiresAtUtc:u}.");
        }

        if (reservation.Status is UsageEventStatus.Released)
            throw new InvalidOperationException($"Cannot commit reservation '{reservationId}' because it is in state '{reservation.Status}'.");

        reservation.Status = UsageEventStatus.Committed;
        reservation.CommittedAtUtc = now;

        var metric = await _db.UsageMetrics
            .FirstOrDefaultAsync(m => m.ClinicId == reservation.ClinicId && m.MetricType == reservation.MetricType && m.PeriodStartUtc == reservation.PeriodStartUtc, ct);

        if (metric is null)
        {
            metric = new UsageMetric
            {
                ClinicId = reservation.ClinicId,
                MetricType = reservation.MetricType,
                PeriodStartUtc = reservation.PeriodStartUtc,
                PeriodEndUtc = reservation.PeriodEndUtc,
                QuotaLimit = 0,
                CurrentValue = reservation.Delta,
                LastUpdatedAtUtc = now
            };
            _db.UsageMetrics.Add(metric);
        }
        else
        {
            metric.CurrentValue += reservation.Delta;
            metric.LastUpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReleaseReservationAsync(Guid reservationId, string? reason = null, CancellationToken ct = default)
    {
        var reservation = await _db.UsageEvents
            .FirstOrDefaultAsync(e => e.Id == reservationId, ct);

        if (reservation is null || reservation.Status == UsageEventStatus.Released)
            return;

        if (reservation.Status == UsageEventStatus.Committed)
            return;

        reservation.Status = UsageEventStatus.Released;
        reservation.ReleasedAtUtc = DateTime.UtcNow;
        reservation.ReleaseReason = reason;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> CleanupExpiredReservationsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var staleReservations = await _db.UsageEvents
            .Where(e => e.Status == UsageEventStatus.Reserved && e.ExpiresAtUtc <= now)
            .ToListAsync(ct);

        if (staleReservations.Count == 0)
            return 0;

        foreach (var r in staleReservations)
        {
            r.Status = UsageEventStatus.Expired;
            r.ReleasedAtUtc = now;
            r.ReleaseReason = "Expired by background cleanup worker.";
        }

        return await _db.SaveChangesAsync(ct);
    }

    private static long ComputeAdvisoryKey(string key)
    {
        unchecked
        {
            long hash = 1125899906842597L;
            foreach (char c in key)
            {
                hash = (hash * 31) ^ c;
            }
            return hash;
        }
    }

    public async Task<ClinicSubscriptionSummaryDto> GetSubscriptionSummaryAsync(Guid clinicId, CancellationToken ct = default)
    {
        var clinic = await _db.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            ?? throw new KeyNotFoundException($"Clinic with ID '{clinicId}' not found.");

        var subscription = await _db.ClinicSubscriptions
            .AsNoTracking()
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.ClinicId == clinicId && s.IsActive)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefaultAsync(ct);

        var enabledFeatures = await GetActiveFeaturesAsync(clinicId, ct);

        var (periodStart, periodEnd) = GetCurrentMonthPeriod();

        // Calculate quotas
        var quotas = new List<UsageQuotaDto>();

        // Doctors
        long docLimit = subscription?.MaxDoctorsSnapshot ?? 3;
        long docCount = await _db.Doctors.CountAsync(d => d.ClinicId == clinicId && !d.IsDeleted, ct);
        quotas.Add(new UsageQuotaDto(
            MetricType.DoctorsCount,
            "Doctors",
            docCount,
            docLimit,
            docLimit > 0 ? Math.Min(100.0, (double)docCount / docLimit * 100.0) : 0.0,
            docLimit > 0 && docCount >= docLimit,
            periodStart,
            periodEnd));

        // Users
        long userLimit = subscription?.MaxUsersSnapshot ?? 5;
        long userCount = await _db.ClinicMembers.CountAsync(m => m.ClinicId == clinicId && !m.IsDeleted, ct);
        quotas.Add(new UsageQuotaDto(
            MetricType.UsersCount,
            "Staff Members",
            userCount,
            userLimit,
            userLimit > 0 ? Math.Min(100.0, (double)userCount / userLimit * 100.0) : 0.0,
            userLimit > 0 && userCount >= userLimit,
            periodStart,
            periodEnd));

        // Patients
        long patientLimit = subscription?.MaxPatientsSnapshot ?? 500;
        long patientCount = await _db.Patients.CountAsync(p => p.ClinicId == clinicId && !p.IsDeleted, ct);
        quotas.Add(new UsageQuotaDto(
            MetricType.PatientsCount,
            "Patients",
            patientCount,
            patientLimit,
            patientLimit > 0 ? Math.Min(100.0, (double)patientCount / patientLimit * 100.0) : 0.0,
            patientLimit > 0 && patientCount >= patientLimit,
            periodStart,
            periodEnd));

        // AI Requests
        long aiLimit = subscription?.MonthlyAiRequestsLimitSnapshot ?? 100;
        var aiMetric = await _db.UsageMetrics
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.AiRequestsCount && m.PeriodStartUtc == periodStart, ct);
        long aiCount = aiMetric?.CurrentValue ?? 0;
        quotas.Add(new UsageQuotaDto(
            MetricType.AiRequestsCount,
            "Monthly AI Inferences",
            aiCount,
            aiLimit,
            aiLimit > 0 ? Math.Min(100.0, (double)aiCount / aiLimit * 100.0) : 0.0,
            aiLimit > 0 && aiCount >= aiLimit,
            periodStart,
            periodEnd));

        // DICOM Studies
        long dicomLimit = subscription?.MaxDicomStudiesMonthlySnapshot ?? 0;
        var dicomMetric = await _db.UsageMetrics
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClinicId == clinicId && m.MetricType == MetricType.DicomStudiesCount && m.PeriodStartUtc == periodStart, ct);
        long dicomCount = dicomMetric?.CurrentValue ?? 0;
        quotas.Add(new UsageQuotaDto(
            MetricType.DicomStudiesCount,
            "Monthly DICOM Studies",
            dicomCount,
            dicomLimit,
            dicomLimit > 0 ? Math.Min(100.0, (double)dicomCount / dicomLimit * 100.0) : 0.0,
            dicomLimit > 0 && dicomCount >= dicomLimit,
            periodStart,
            periodEnd));

        return new ClinicSubscriptionSummaryDto(
            ClinicId: clinic.Id,
            ClinicName: clinic.Name,
            LifecycleStatus: clinic.LifecycleStatus,
            BillingStatus: clinic.BillingStatus,
            ComplianceStatus: clinic.ComplianceStatus,
            SuspensionReason: clinic.SuspensionReason,
            PlanCode: subscription?.PlanCodeSnapshot ?? clinic.Plan.ToString().ToLowerInvariant(),
            PlanName: subscription?.SubscriptionPlan?.Name ?? $"{clinic.Plan} Plan",
            Tier: subscription?.Tier ?? SubscriptionTier.Basic,
            BillingCycle: subscription?.BillingCycle ?? BillingCycle.Monthly,
            SubscriptionStatus: subscription?.Status ?? SubscriptionStatus.Active,
            TrialEndsAt: clinic.TrialEndsAt,
            NextBillingDateUtc: subscription?.NextBillingDateUtc,
            GracePeriodEndsAtUtc: subscription?.GracePeriodEndsAtUtc,
            EnabledFeatures: enabledFeatures,
            Quotas: quotas
        );
    }

    private static (DateTime start, DateTime end) GetCurrentMonthPeriod()
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        return (start, end);
    }
}
