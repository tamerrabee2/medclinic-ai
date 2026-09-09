namespace MedClinic.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? ClinicId { get; }
    string? Role { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}
