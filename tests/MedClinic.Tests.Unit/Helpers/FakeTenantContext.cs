using MedClinic.Application.Interfaces;

namespace MedClinic.Tests.Unit.Helpers;

public class FakeTenantContext : ITenantContext
{
    public Guid ClinicId { get; set; } = Guid.NewGuid();
    public Guid UserId   { get; set; } = Guid.NewGuid();

    Guid? ITenantContext.ClinicId => ClinicId;
    Guid? ITenantContext.UserId   => UserId;

    public string? ClinicName { get; set; } = "Test Clinic";
    public bool IsAuthenticated { get; set; } = true;
    public IEnumerable<string> Roles { get; set; } = ["Doctor"];
    public bool IsSuperAdmin { get; set; } = false;
    public string Language { get; set; } = "en";
    public string TimeZone { get; set; } = "UTC";

    public bool IsInRole(string role) => Roles.Contains(role);
}
