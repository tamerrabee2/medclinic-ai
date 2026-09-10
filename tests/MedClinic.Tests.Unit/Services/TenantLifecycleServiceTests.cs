using FluentAssertions;
using MedClinic.Application.Features.SuperAdmin.DTOs;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Services;
using MedClinic.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class TenantLifecycleServiceTests
{
    private static void SeedPlans(MedClinic.Infrastructure.Persistence.ApplicationDbContext db)
    {
        db.SubscriptionPlans.AddRange(
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Code = "basic",
                Name = "Basic Clinic",
                Tier = SubscriptionTier.Basic,
                MonthlyPrice = 49m,
                AnnualPrice = 490m,
                MaxDoctors = 3,
                MaxUsers = 5,
                MaxPatients = 500,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Code = "pro",
                Name = "Pro Clinic",
                Tier = SubscriptionTier.Pro,
                MonthlyPrice = 149m,
                AnnualPrice = 1490m,
                MaxDoctors = 15,
                MaxUsers = 25,
                MaxPatients = 5000,
                IsActive = true
            }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task ProvisionClinicAsync_CreatesClinic_Subscription_AdminUser_AndAuditTrail()
    {
        using var db = TestDbContextFactory.Create();
        SeedPlans(db);

        var sut = new TenantLifecycleService(db);

        var request = new ProvisionClinicRequest(
            Name: "Nile Care Clinic",
            Slug: "nile-care",
            Description: "Specialized cardiology clinic",
            Email: "contact@nilecare.com",
            Phone: "+201000000000",
            Address: "15 Nile Corniche",
            City: "Cairo",
            Country: "EG",
            Currency: "USD",
            Timezone: "Africa/Cairo",
            PlanCode: "pro",
            BillingCycle: BillingCycle.Monthly,
            TrialDays: 14,
            AdminUser: new AdminUserProvisionDto(
                Email: "admin@nilecare.com",
                FirstName: "Karim",
                LastName: "Fouad",
                Password: "SecurePassword123!"
            )
        );

        var clinic = await sut.ProvisionClinicAsync(request);

        clinic.Should().NotBeNull();
        clinic.Name.Should().Be("Nile Care Clinic");
        clinic.Slug.Should().Be("nile-care");
        clinic.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Trial);
        clinic.TrialEndsAt.Should().NotBeNull();

        // Check subscription snapshot
        var sub = await db.ClinicSubscriptions.FirstOrDefaultAsync(s => s.ClinicId == clinic.Id);
        sub.Should().NotBeNull();
        sub!.PlanCodeSnapshot.Should().Be("pro");
        sub.MonthlyPriceSnapshot.Should().Be(149m);
        sub.MaxDoctorsSnapshot.Should().Be(15);

        // Check Admin User
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == "admin@nilecare.com");
        admin.Should().NotBeNull();
        admin!.FirstName.Should().Be("Karim");

        var member = await db.ClinicMembers.FirstOrDefaultAsync(m => m.ClinicId == clinic.Id && m.UserId == admin.Id);
        member.Should().NotBeNull();

        // Check Audit Trail
        var audit = await db.TenantLifecycleAuditEvents.FirstOrDefaultAsync(a => a.ClinicId == clinic.Id);
        audit.Should().NotBeNull();
        audit!.EventType.Should().Be(TenantLifecycleEventType.Created);
        audit.NewValue.Should().Contain("nile-care");
    }

    [Fact]
    public async Task SuspendClinicAsync_UpdatesLifecycle_AndLogsAuditEvent()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();
        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Active Clinic",
            Slug = "active-clinic",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });
        await db.SaveChangesAsync();

        var sut = new TenantLifecycleService(db);
        var adminUserId = Guid.NewGuid();

        await sut.SuspendClinicAsync(clinicId, "Unresolved terms violation", adminUserId);

        var clinic = await db.Clinics.FindAsync(clinicId);
        clinic!.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Suspended);
        clinic.SuspensionReason.Should().Be("Unresolved terms violation");
        clinic.SuspendedAt.Should().NotBeNull();
        clinic.SuspendedByUserId.Should().Be(adminUserId);

        var audit = await db.TenantLifecycleAuditEvents
            .FirstOrDefaultAsync(a => a.ClinicId == clinicId && a.EventType == TenantLifecycleEventType.Suspended);
        audit.Should().NotBeNull();
        audit!.OldValue.Should().Be(ClinicLifecycleStatus.Active.ToString());
        audit.NewValue.Should().Be(ClinicLifecycleStatus.Suspended.ToString());
        audit.Reason.Should().Be("Unresolved terms violation");
    }

    [Fact]
    public async Task ReactivateClinicAsync_RestoresActiveStatus_AndClearsSuspensionFields()
    {
        using var db = TestDbContextFactory.Create();
        var clinicId = Guid.NewGuid();
        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Suspended Clinic",
            Slug = "suspended-clinic",
            LifecycleStatus = ClinicLifecycleStatus.Suspended,
            SuspendedAt = DateTime.UtcNow.AddDays(-2),
            SuspensionReason = "Billing issue"
        });
        await db.SaveChangesAsync();

        var sut = new TenantLifecycleService(db);

        await sut.ReactivateClinicAsync(clinicId, "Payment received and cleared");

        var clinic = await db.Clinics.FindAsync(clinicId);
        clinic!.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Active);
        clinic.SuspendedAt.Should().BeNull();
        clinic.SuspensionReason.Should().BeNull();

        var audit = await db.TenantLifecycleAuditEvents
            .FirstOrDefaultAsync(a => a.ClinicId == clinicId && a.EventType == TenantLifecycleEventType.Reactivated);
        audit.Should().NotBeNull();
        audit!.Reason.Should().Be("Payment received and cleared");
    }

    [Fact]
    public async Task ChangePlanAsync_ArchivesOldSubscription_AndCreatesNewSnapshot()
    {
        using var db = TestDbContextFactory.Create();
        SeedPlans(db);

        var basicPlan = await db.SubscriptionPlans.FirstAsync(p => p.Code == "basic");
        var proPlan = await db.SubscriptionPlans.FirstAsync(p => p.Code == "pro");

        var clinicId = Guid.NewGuid();
        db.Clinics.Add(new Clinic
        {
            Id = clinicId,
            Name = "Growing Clinic",
            Slug = "growing-clinic",
            LifecycleStatus = ClinicLifecycleStatus.Active
        });

        var initialSub = ClinicSubscription.CreateWithSnapshot(clinicId, basicPlan, BillingCycle.Monthly);
        db.ClinicSubscriptions.Add(initialSub);
        await db.SaveChangesAsync();

        var sut = new TenantLifecycleService(db);

        await sut.ChangePlanAsync(clinicId, proPlan.Id, BillingCycle.Annual);

        var subscriptions = await db.ClinicSubscriptions
            .Where(s => s.ClinicId == clinicId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        subscriptions.Should().HaveCount(2);
        subscriptions[0].IsActive.Should().BeFalse(); // Old plan archived
        subscriptions[1].IsActive.Should().BeTrue();  // New plan active
        subscriptions[1].PlanCodeSnapshot.Should().Be("pro");
        subscriptions[1].BillingCycle.Should().Be(BillingCycle.Annual);
        subscriptions[1].AnnualPriceSnapshot.Should().Be(1490m);
    }
}
