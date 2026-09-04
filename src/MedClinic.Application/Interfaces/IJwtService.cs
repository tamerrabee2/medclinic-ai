using MedClinic.Domain.Entities;
using System.Security.Claims;

namespace MedClinic.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateAccessTokenWithClinic(ApplicationUser user, IList<string> roles, Guid clinicId);
    RefreshToken GenerateRefreshToken(Guid userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
