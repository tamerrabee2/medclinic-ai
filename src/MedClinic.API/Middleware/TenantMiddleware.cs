using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedClinic.API.Middleware;

/// <summary>
/// Resolves and validates the current tenant (clinic) from:
/// 1. X-Clinic-Id request header
/// 2. clinic_id JWT claim
/// Validates that the authenticated user is an active member of that clinic.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    // Routes that don't require a clinic context
    private static readonly HashSet<string> _exemptPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/auth",
        "/health",
        "/swagger",
        "/favicon.ico"
    };

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip exempt routes
        if (_exemptPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Skip unauthenticated requests (auth middleware handles that)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Resolve ClinicId from header or JWT claim
        var clinicIdStr = context.Request.Headers["X-Clinic-Id"].FirstOrDefault()
            ?? context.User.FindFirstValue("clinic_id");

        if (string.IsNullOrWhiteSpace(clinicIdStr) || !Guid.TryParse(clinicIdStr, out var clinicId))
        {
            // Not all routes need a clinic — allow through, controllers enforce via ITenantContext
            await _next(context);
            return;
        }

        // Validate clinic exists and is active
        var clinic = await db.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicId && c.IsActive && !c.IsDeleted);

        if (clinic == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Clinic not found or inactive."
            });
            return;
        }

        // Validate user is a member of this clinic (SuperAdmin bypasses)
        var isSuperAdmin = context.User.IsInRole(MedClinic.Shared.Constants.Roles.SuperAdmin);
        if (!isSuperAdmin)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userGuid))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid user identity." });
                return;
            }

            var isMember = await db.ClinicMembers
                .AsNoTracking()
                .AnyAsync(cm =>
                    cm.ClinicId == clinicId &&
                    cm.UserId == userGuid &&
                    !cm.IsDeleted);

            if (!isMember)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access Clinic {ClinicId} without membership",
                    userId, clinicId);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "You are not a member of this clinic."
                });
                return;
            }
        }

        // Store resolved clinicId in HttpContext items for ITenantContext
        context.Items["ClinicId"] = clinicId;
        context.Items["ClinicName"] = clinic.Name;

        _logger.LogDebug("Tenant resolved: {ClinicName} ({ClinicId})", clinic.Name, clinicId);

        await _next(context);
    }
}
