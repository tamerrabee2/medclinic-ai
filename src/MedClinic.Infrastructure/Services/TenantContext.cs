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
                .FindFirstValue("clinicId");
            return clinicIdClaim != null && Guid.TryParse(clinicIdClaim, out var id)
                ? id
                : null;
        }
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim, out var id)
                ? id
                : null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public IList<string> Roles =>
        _httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];
}
