using System.Text.Json;
using MedClinic.Application.Features.SuperAdmin.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[ApiController]
[Route("api/v1/superadmin")]
[Authorize(Roles = Roles.SuperAdmin)]
public class SuperAdminController : BaseController
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantLifecycleService _lifecycleService;
    private readonly ITenantEntitlementService _entitlementService;

    public SuperAdminController(
        ApplicationDbContext db,
        ITenantLifecycleService lifecycleService,
        ITenantEntitlementService entitlementService)
    {
        _db = db;
        _lifecycleService = lifecycleService;
        _entitlementService = entitlementService;
    }

    /// <summary>
    /// Global Platform Overview KPIs for Super Administrators.
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var totalClinics = await _db.Clinics.CountAsync(ct);
        var activeClinics = await _db.Clinics.CountAsync(c => c.LifecycleStatus == ClinicLifecycleStatus.Active, ct);
        var trialClinics = await _db.Clinics.CountAsync(c => c.LifecycleStatus == ClinicLifecycleStatus.Trial, ct);
        var suspendedClinics = await _db.Clinics.CountAsync(c => c.LifecycleStatus == ClinicLifecycleStatus.Suspended, ct);
        var cancelledClinics = await _db.Clinics.CountAsync(c => c.LifecycleStatus == ClinicLifecycleStatus.Cancelled, ct);

        var totalDoctors = await _db.Doctors.CountAsync(d => !d.IsDeleted, ct);
        var totalPatients = await _db.Patients.CountAsync(p => !p.IsDeleted, ct);

        var totalAiRequests = await _db.UsageMetrics
            .Where(m => m.MetricType == MetricType.AiRequestsCount)
            .SumAsync(m => m.CurrentValue, ct);

        var totalDicomStudies = await _db.UsageMetrics
            .Where(m => m.MetricType == MetricType.DicomStudiesCount)
            .SumAsync(m => m.CurrentValue, ct);

        var activeSubs = await _db.ClinicSubscriptions
            .Where(s => s.IsActive)
            .GroupBy(s => s.Tier)
            .Select(g => new { Tier = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Tier, x => x.Count, ct);

        var dto = new SuperAdminOverviewDto(
            TotalClinics: totalClinics,
            ActiveClinics: activeClinics,
            TrialClinics: trialClinics,
            SuspendedClinics: suspendedClinics,
            CancelledClinics: cancelledClinics,
            TotalDoctors: totalDoctors,
            TotalPatients: totalPatients,
            TotalAiRequests: totalAiRequests,
            TotalDicomStudies: totalDicomStudies,
            ClinicsByTier: activeSubs
        );

        return Success(dto);
    }

    /// <summary>
    /// Search and list all tenants with pagination and lifecycle filtering.
    /// </summary>
    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants(
        [FromQuery] string? search,
        [FromQuery] ClinicLifecycleStatus? status,
        [FromQuery] BillingStatus? billingStatus,
        [FromQuery] SubscriptionTier? tier,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var query = _db.Clinics.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) || c.Slug.ToLower().Contains(s) || (c.Email != null && c.Email.ToLower().Contains(s)));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.LifecycleStatus == status.Value);
        }

        if (billingStatus.HasValue)
        {
            query = query.Where(c => c.BillingStatus == billingStatus.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var clinics = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                Clinic = c,
                ActiveSub = c.Subscriptions.Where(s => s.IsActive).OrderByDescending(s => s.StartDateUtc).FirstOrDefault(),
                DoctorsCount = c.Doctors.Count(d => !d.IsDeleted),
                PatientsCount = c.Patients.Count(p => !p.IsDeleted)
            })
            .ToListAsync(ct);

        var items = clinics.Select(x => new TenantListItemDto(
            Id: x.Clinic.Id,
            Name: x.Clinic.Name,
            Slug: x.Clinic.Slug,
            Email: x.Clinic.Email,
            Phone: x.Clinic.Phone,
            City: x.Clinic.City,
            Country: x.Clinic.Country,
            LifecycleStatus: x.Clinic.LifecycleStatus,
            BillingStatus: x.Clinic.BillingStatus,
            ComplianceStatus: x.Clinic.ComplianceStatus,
            TrialEndsAt: x.Clinic.TrialEndsAt,
            SuspendedAt: x.Clinic.SuspendedAt,
            SuspensionReason: x.Clinic.SuspensionReason,
            PlanCode: x.ActiveSub?.PlanCodeSnapshot ?? x.Clinic.Plan.ToString().ToLowerInvariant(),
            Tier: x.ActiveSub?.Tier ?? (x.Clinic.Plan == ClinicPlan.Enterprise ? SubscriptionTier.Enterprise : x.Clinic.Plan == ClinicPlan.Professional ? SubscriptionTier.Pro : SubscriptionTier.Basic),
            DoctorsCount: x.DoctorsCount,
            PatientsCount: x.PatientsCount,
            CreatedAt: x.Clinic.CreatedAt
        )).ToList();

        return Success(new
        {
            Total = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        });
    }

    /// <summary>
    /// Gets complete detail of a tenant clinic including active subscription and recent lifecycle events.
    /// </summary>
    [HttpGet("tenants/{clinicId:guid}")]
    public async Task<IActionResult> GetTenantDetail(Guid clinicId, CancellationToken ct)
    {
        var clinic = await _db.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId, ct);

        if (clinic is null)
            return NotFound("Clinic not found.");

        var summary = await _entitlementService.GetSubscriptionSummaryAsync(clinicId, ct);
        var auditTrail = await _lifecycleService.GetLifecycleAuditTrailAsync(clinicId, ct);

        return Success(new
        {
            Clinic = new
            {
                clinic.Id,
                clinic.Name,
                clinic.Slug,
                clinic.Description,
                clinic.LogoUrl,
                clinic.Email,
                clinic.Phone,
                clinic.Address,
                clinic.City,
                clinic.Country,
                clinic.TimeZone,
                clinic.Currency,
                clinic.LifecycleStatus,
                clinic.BillingStatus,
                clinic.ComplianceStatus,
                clinic.TrialEndsAt,
                clinic.SuspendedAt,
                clinic.SuspensionReason,
                clinic.CreatedAt
            },
            Subscription = summary,
            RecentAuditEvents = auditTrail.Take(10).ToList()
        });
    }

    /// <summary>
    /// Provisions a new clinic with trial or active subscription.
    /// </summary>
    [HttpPost("tenants/provision")]
    public async Task<IActionResult> ProvisionTenant([FromBody] ProvisionClinicRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var clinic = await _lifecycleService.ProvisionClinicAsync(request, CurrentUserId, ip, ct);
        return Created(new
        {
            clinic.Id,
            clinic.Name,
            clinic.Slug,
            clinic.Email,
            clinic.Phone,
            clinic.LifecycleStatus,
            clinic.BillingStatus,
            clinic.ComplianceStatus,
            clinic.TrialEndsAt,
            clinic.CreatedAt
        }, "Clinic provisioned successfully.");
    }

    /// <summary>
    /// Suspends clinic operational access while preserving read-only medical records and compliance rights.
    /// </summary>
    [HttpPost("tenants/{clinicId:guid}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid clinicId, [FromBody] SuspendClinicRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _lifecycleService.SuspendClinicAsync(clinicId, request.Reason, CurrentUserId, ip, request.SuspensionCode, ct);
        return Success(new { clinicId, status = ClinicLifecycleStatus.Suspended }, "Clinic suspended successfully.");
    }

    /// <summary>
    /// Reactivates a suspended clinic.
    /// </summary>
    [HttpPost("tenants/{clinicId:guid}/reactivate")]
    public async Task<IActionResult> ReactivateTenant(Guid clinicId, [FromBody] ReactivateClinicRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _lifecycleService.ReactivateClinicAsync(clinicId, request.Reason, CurrentUserId, ip, null, ct);
        return Success(new { clinicId, status = ClinicLifecycleStatus.Active }, "Clinic reactivated successfully.");
    }

    /// <summary>
    /// Cancels a clinic and terminates its active subscriptions.
    /// </summary>
    [HttpPost("tenants/{clinicId:guid}/cancel")]
    public async Task<IActionResult> CancelTenant(Guid clinicId, [FromBody] SuspendClinicRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _lifecycleService.CancelClinicAsync(clinicId, request.Reason, CurrentUserId, ip, null, ct);
        return Success(new { clinicId, status = ClinicLifecycleStatus.Cancelled }, "Clinic cancelled successfully.");
    }

    /// <summary>
    /// Changes the subscription plan of a tenant.
    /// </summary>
    [HttpPost("tenants/{clinicId:guid}/plan")]
    public async Task<IActionResult> ChangeTenantPlan(Guid clinicId, [FromBody] ChangeClinicPlanRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _lifecycleService.ChangePlanAsync(clinicId, request.NewPlanId, request.BillingCycle, CurrentUserId, ip, ct);
        return Success(new { clinicId, newPlanId = request.NewPlanId }, "Tenant plan changed successfully.");
    }

    /// <summary>
    /// Sets compliance status (LegalHold, Restricted, Normal) for a tenant.
    /// </summary>
    [HttpPost("tenants/{clinicId:guid}/compliance")]
    public async Task<IActionResult> SetTenantCompliance(Guid clinicId, [FromBody] SetClinicComplianceRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _lifecycleService.SetComplianceStatusAsync(clinicId, request.Status, request.Reason, CurrentUserId, ip, ct);
        return Success(new { clinicId, complianceStatus = request.Status }, "Tenant compliance status updated successfully.");
    }

    /// <summary>
    /// Gets the immutable tenant lifecycle audit trail.
    /// </summary>
    [HttpGet("tenants/{clinicId:guid}/lifecycle-audit")]
    public async Task<IActionResult> GetTenantAuditTrail(Guid clinicId, CancellationToken ct)
    {
        var events = await _lifecycleService.GetLifecycleAuditTrailAsync(clinicId, ct);
        return Success(events);
    }

    /// <summary>
    /// Catalog of all subscription plans.
    /// </summary>
    [HttpGet("plans")]
    public async Task<IActionResult> GetAllPlans(CancellationToken ct)
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);

        return Success(plans);
    }

    /// <summary>
    /// Creates a new subscription plan in the platform catalog.
    /// </summary>
    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateOrUpdatePlanRequest request, CancellationToken ct)
    {
        var code = request.Code.Trim().ToLowerInvariant();
        if (await _db.SubscriptionPlans.AnyAsync(p => p.Code == code, ct))
            return BadRequest($"Plan code '{code}' is already in use.");

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Tier = request.Tier,
            MonthlyPrice = request.MonthlyPrice,
            AnnualPrice = request.AnnualPrice,
            Currency = request.Currency,
            MaxDoctors = request.MaxDoctors,
            MaxUsers = request.MaxUsers,
            MaxPatients = request.MaxPatients,
            MaxStorageBytes = request.MaxStorageBytes,
            MonthlyAiRequestsLimit = request.MonthlyAiRequestsLimit,
            MaxDicomStudiesMonthly = request.MaxDicomStudiesMonthly,
            FeaturesJson = JsonSerializer.Serialize(request.Features ?? []),
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.SubscriptionPlans.Add(plan);
        await _db.SaveChangesAsync(ct);

        return Created(plan, "Subscription plan created successfully.");
    }

    /// <summary>
    /// Updates an existing subscription plan in the platform catalog.
    /// Note: Does not mutate existing snapshots on active clinic subscriptions.
    /// </summary>
    [HttpPut("plans/{id:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] CreateOrUpdatePlanRequest request, CancellationToken ct)
    {
        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (plan is null)
            return NotFound("Subscription plan not found.");

        plan.Name = request.Name.Trim();
        plan.Description = request.Description?.Trim();
        plan.Tier = request.Tier;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.AnnualPrice = request.AnnualPrice;
        plan.Currency = request.Currency;
        plan.MaxDoctors = request.MaxDoctors;
        plan.MaxUsers = request.MaxUsers;
        plan.MaxPatients = request.MaxPatients;
        plan.MaxStorageBytes = request.MaxStorageBytes;
        plan.MonthlyAiRequestsLimit = request.MonthlyAiRequestsLimit;
        plan.MaxDicomStudiesMonthly = request.MaxDicomStudiesMonthly;
        plan.FeaturesJson = JsonSerializer.Serialize(request.Features ?? []);
        plan.IsActive = request.IsActive;
        plan.DisplayOrder = request.DisplayOrder;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Success(plan, "Subscription plan updated successfully.");
    }
}
