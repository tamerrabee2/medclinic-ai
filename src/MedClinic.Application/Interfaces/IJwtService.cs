using System.Security.Claims;
using MedClinic.Domain.Entities;

namespace MedClinic.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, Guid? clinicId = null);
    RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress = null, string? deviceInfo = null);
    ClaimsPrincipal? ValidateToken(string token);
}
