using MedClinic.Application.Features.SuperAdmin.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Services;

public sealed class TenantLifecycleService : ITenantLifecycleService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<ApplicationUser> _hasher;

    public TenantLifecycleService(ApplicationDbContext db, IPasswordHasher<ApplicationUser>? hasher = null)
    {
        _db = db;
        _hasher = hasher ?? new PasswordHasher<ApplicationUser>();
    }

    public async Task<Clinic> ProvisionClinicAsync(
        ProvisionClinicRequest request,
        Guid? performedByUserId = null,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _db.Clinics.AnyAsync(c => c.Slug == slug, ct))
        {
            throw new InvalidOperationException($"Clinic with slug '{slug}' already exists.");
        }

        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == request.PlanCode && p.IsActive, ct)
            ?? await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == "pro", ct)
            ?? await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Code == "basic", ct)
            ?? throw new InvalidOperationException("No subscription plans found in system database.");

        var clinicId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var trialEndsAt = request.TrialDays > 0 ? now.AddDays(request.TrialDays) : (DateTime?)null;

        var clinic = new Clinic
        {
            Id = clinicId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            Country = request.Country?.Trim() ?? "US",
            Currency = request.Currency,
            TimeZone = request.Timezone,
            LifecycleStatus = trialEndsAt.HasValue ? ClinicLifecycleStatus.Trial : ClinicLifecycleStatus.Active,
            BillingStatus = BillingStatus.Current,
            ComplianceStatus = TenantComplianceStatus.Normal,
            TrialEndsAt = trialEndsAt,
            Plan = plan.Tier switch
            {
                SubscriptionTier.Enterprise => ClinicPlan.Enterprise,
                SubscriptionTier.Pro => ClinicPlan.Professional,
                _ => ClinicPlan.Basic
            },
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Clinics.Add(clinic);

        // Create initial subscription with frozen snapshots
        var subscription = ClinicSubscription.CreateWithSnapshot(
            clinicId,
            plan,
            request.BillingCycle,
            SubscriptionStatus.Active,
            BillingStatus.Current,
            trialEndsAt);

        _db.ClinicSubscriptions.Add(subscription);

        // Optionally provision initial admin user
        if (request.AdminUser is not null)
        {
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = request.AdminUser.FirstName.Trim(),
                LastName = request.AdminUser.LastName.Trim(),
                Email = request.AdminUser.Email.Trim().ToLowerInvariant(),
                UserName = request.AdminUser.Email.Trim().ToLowerInvariant(),
                NormalizedEmail = request.AdminUser.Email.Trim().ToUpperInvariant(),
                NormalizedUserName = request.AdminUser.Email.Trim().ToUpperInvariant(),
                PhoneNumber = request.AdminUser.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = now,
                UpdatedAt = now
            };
            adminUser.PasswordHash = _hasher.HashPassword(adminUser, request.AdminUser.Password);

            _db.Users.Add(adminUser);

            var member = new ClinicMember
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                UserId = adminUser.Id,
                Role = Roles.ClinicAdmin,
                IsActive = true,
                JoinedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.ClinicMembers.Add(member);
        }

        // Add Immutable Audit Log
        _db.TenantLifecycleAuditEvents.Add(new TenantLifecycleAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            EventType = TenantLifecycleEventType.Created,
            OldValue = null,
            NewValue = $"Provisioned with {plan.Name} (Slug: {slug}, Plan: {plan.Code}, Cycle: {request.BillingCycle}, TrialDays: {request.TrialDays})",
            Reason = "Initial clinic provisioning",
            PerformedByUserId = performedByUserId,
            IpAddress = ipAddress,
            Timestamp = now
        });

        await _db.SaveChangesAsync(ct);
        return clinic;
    }

    public async Task SuspendClinicAsync(
        Guid clinicId,
        string reason,
        Guid? performedByUserId = null,
        string? ipAddress = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            ?? throw new KeyNotFoundException($"Clinic with ID '{clinicId}' not found.");

        var oldStatus = clinic.LifecycleStatus;
        var now = DateTime.UtcNow;

        clinic.LifecycleStatus = ClinicLifecycleStatus.Suspended;
        clinic.SuspendedAt = now;
        clinic.SuspensionReason = reason.Trim();
        clinic.SuspendedByUserId = performedByUserId;
        clinic.UpdatedAt = now;

        _db.TenantLifecycleAuditEvents.Add(new TenantLifecycleAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            EventType = TenantLifecycleEventType.Suspended,
            OldValue = oldStatus.ToString(),
            NewValue = ClinicLifecycleStatus.Suspended.ToString(),
            Reason = reason.Trim(),
            PerformedByUserId = performedByUserId,
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            Timestamp = now
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReactivateClinicAsync(
        Guid clinicId,
        string? reason = null,
        Guid? performedByUserId = null,
        string? ipAddress = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            ?? throw new KeyNotFoundException($"Clinic with ID '{clinicId}' not found.");

        var oldStatus = clinic.LifecycleStatus;
        var now = DateTime.UtcNow;

        clinic.LifecycleStatus = ClinicLifecycleStatus.Active;
        clinic.SuspendedAt = null;
        clinic.SuspensionReason = null;
        clinic.SuspendedByUserId = null;
        clinic.UpdatedAt = now;

        _db.TenantLifecycleAuditEvents.Add(new TenantLifecycleAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            EventType = TenantLifecycleEventType.Reactivated,
            OldValue = oldStatus.ToString(),
            NewValue = ClinicLifecycleStatus.Active.ToString(),
            Reason = reason?.Trim() ?? "Clinic operations reactivated by administrator",
            PerformedByUserId = performedByUserId,
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            Timestamp = now
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelClinicAsync(
        Guid clinicId,
        string reason,
        Guid? performedByUserId = null,
        string? ipAddress = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            ?? throw new KeyNotFoundException($"Clinic with ID '{clinicId}' not found.");

        var oldStatus = clinic.LifecycleStatus;
        var now = DateTime.UtcNow;

        clinic.LifecycleStatus = ClinicLifecycleStatus.Cancelled;
        clinic.SuspendedAt = now;
        clinic.SuspensionReason = reason.Trim();
        clinic.UpdatedAt = now;

        // Cancel active subscriptions
        var activeSubscriptions = await _db.ClinicSubscriptions
            .Where(s => s.ClinicId == clinicId && s.IsActive)
            .ToListAsync(ct);

        foreach (var sub in activeSubscriptions)
        {
            sub.IsActive = false;
            sub.Status = SubscriptionStatus.Cancelled;
            sub.CancelledAtUtc = now;
            sub.CancellationReason = reason.Trim();
            sub.UpdatedAt = now;
        }

        _db.TenantLifecycleAuditEvents.Add(new TenantLifecycleAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            EventType = TenantLifecycleEventType.Cancelled,
            OldValue = oldStatus.ToString(),
            NewValue = ClinicLifecycleStatus.Cancelled.ToString(),
            Reason = reason.Trim(),
            PerformedByUserId = performedByUserId,
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            Timestamp = now
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task ChangePlanAsync(
        Guid clinicId,
        Guid newPlanId,
        BillingCycle cycle,
        Guid? performedByUserId = null,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            ?? throw new KeyNotFoundException($"Clinic with ID '{clinicId}' not found.");

        var newPlan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == newPlanId, ct)
            ?? throw new KeyNotFoundException($"Subscription Plan with ID '{newPlanId}' not found.");

        var now = DateTime.UtcNow;

        // Archive previous subscriptions
        var existingSubscriptions = await _db.ClinicSubscriptions
            .Where(s => s.ClinicId == clinicId && s.IsActive)
            .ToListAsync(ct);

        string oldPlan = existingSubscriptions.FirstOrDefault()?.PlanCodeSnapshot ?? "none";

        foreach (var s in existingSubscriptions)
        {
            s.IsActive = false;
            s.UpdatedAt = now;
        }

        // Create new subscription with snapshot of newPlan
        var newSub = ClinicSubscription.CreateWithSnapshot(
            clinicId,
            newPlan,
            cycle,
            SubscriptionStatus.Active,
            BillingStatus.Current);

        _db.ClinicSubscriptions.Add(newSub);

        // Sync legacy plan
        clinic.Plan = newPlan.Tier switch
        {
            SubscriptionTier.Enterprise => ClinicPlan.Enterprise,
            SubscriptionTier.Pro => ClinicPlan.Professional,
            _ => ClinicPlan.Basic
        };
        clinic.UpdatedAt = now;

        _db.TenantLifecycleAuditEvents.Add(new TenantLifecycleAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            EventType = TenantLifecycleEventType.PlanChanged,
            OldValue = oldPlan,
            NewValue = newPlan.Code,
            Reason = $"Plan changed to {newPlan.Name} ({cycle})",
            PerformedByUserId = performedByUserId,
            IpAddress = ipAddress,
            Timestamp = now
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task SetComplianceStatusAsync(
        Guid clinicId,
        TenantComplianceStatus status,
        string reason,
        Guid? performedByUserId = null,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId, ct)
            ?? throw new KeyNotFoundException($"Clinic with ID '{clinicId}' not found.");

        var oldStatus = clinic.ComplianceStatus;
        var now = DateTime.UtcNow;

        clinic.ComplianceStatus = status;
        clinic.UpdatedAt = now;

        var eventType = status switch
        {
            TenantComplianceStatus.LegalHold => TenantLifecycleEventType.ComplianceLegalHold,
            TenantComplianceStatus.Restricted => TenantLifecycleEventType.ComplianceRestricted,
            _ => TenantLifecycleEventType.ComplianceRestored
        };

        _db.TenantLifecycleAuditEvents.Add(new TenantLifecycleAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            EventType = eventType,
            OldValue = oldStatus.ToString(),
            NewValue = status.ToString(),
            Reason = reason.Trim(),
            PerformedByUserId = performedByUserId,
            IpAddress = ipAddress,
            Timestamp = now
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TenantLifecycleAuditEventDto>> GetLifecycleAuditTrailAsync(
        Guid clinicId,
        CancellationToken ct = default)
    {
        var events = await _db.TenantLifecycleAuditEvents
            .AsNoTracking()
            .Where(e => e.ClinicId == clinicId)
            .OrderByDescending(e => e.Timestamp)
            .Select(e => new TenantLifecycleAuditEventDto(
                e.Id,
                e.ClinicId,
                e.EventType,
                e.EventType.ToString(),
                e.OldValue,
                e.NewValue,
                e.Reason,
                e.PerformedByUserId,
                e.PerformedByUserName,
                e.Timestamp,
                e.IpAddress,
                e.CorrelationId
            ))
            .ToListAsync(ct);

        return events;
    }
}
