using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
[Route("api/v1/subscriptions")]
public class SubscriptionsController : BaseController
{
    private readonly ITenantEntitlementService _entitlementService;
    private readonly ApplicationDbContext _db;
    private readonly ITenantContext _tenant;

    public SubscriptionsController(
        ITenantEntitlementService entitlementService,
        ApplicationDbContext db,
        ITenantContext tenant)
    {
        _entitlementService = entitlementService;
        _db = db;
        _tenant = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Tenant clinic context is required.");

    /// <summary>
    /// Gets the comprehensive subscription, lifecycle, and quota utilization status for the current clinic.
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscription(CancellationToken ct)
    {
        var summary = await _entitlementService.GetSubscriptionSummaryAsync(ClinicId, ct);
        return Success(summary);
    }

    /// <summary>
    /// Gets all active subscription plans available for upgrade/downgrade in the catalog.
    /// </summary>
    [HttpGet("plans")]
    public async Task<IActionResult> GetAvailablePlans(CancellationToken ct)
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Id,
                p.Code,
                p.Name,
                p.Description,
                p.Tier,
                p.MonthlyPrice,
                p.AnnualPrice,
                p.Currency,
                p.MaxDoctors,
                p.MaxUsers,
                p.MaxPatients,
                p.MaxStorageBytes,
                p.MonthlyAiRequestsLimit,
                p.MaxDicomStudiesMonthly,
                p.FeaturesJson
            })
            .ToListAsync(ct);

        return Success(plans);
    }

    /// <summary>
    /// Gets the list of active feature keys enabled for the current clinic.
    /// </summary>
    [HttpGet("features")]
    public async Task<IActionResult> GetActiveFeatures(CancellationToken ct)
    {
        var features = await _entitlementService.GetActiveFeaturesAsync(ClinicId, ct);
        return Success(features);
    }
}
