using MedClinic.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MedClinic.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? ClinicId
    {
        get
        {
            // First: from TenantMiddleware resolved value
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("ClinicId", out var item) == true
                && item is Guid resolvedId)
                return resolvedId;

            // Fallback: from header
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-Clinic-Id"].ToString();
            if (!string.IsNullOrWhiteSpace(headerValue) && Guid.TryParse(headerValue, out var headerId))
                return headerId;

            // Fallback: from JWT claim
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue("clinic_id");
            if (Guid.TryParse(claim, out var claimId))
                return claimId;

            return null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? ClinicName =>
        _httpContextAccessor.HttpContext?.Items["ClinicName"] as string;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? [];

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;

    public bool IsSuperAdmin =>
        IsInRole(MedClinic.Shared.Constants.Roles.SuperAdmin);
}
