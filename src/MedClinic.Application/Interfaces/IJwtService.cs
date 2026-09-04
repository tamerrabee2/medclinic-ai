using MedClinic.Domain.Entities;

namespace MedClinic.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
}
