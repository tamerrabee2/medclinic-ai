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
            var clinicIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue("clinic_id");

            if (Guid.TryParse(clinicIdClaim, out var clinicId))
                return clinicId;

            // Also check request header for API clients
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-Clinic-Id"].ToString();
            if (!string.IsNullOrWhiteSpace(headerValue) && Guid.TryParse(headerValue, out var headerClinicId))
                return headerClinicId;

            return null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? [];
}
