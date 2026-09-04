using MedClinic.Application.Interfaces;

namespace MedClinic.Tests.Unit.Helpers;

public class FakeTenantContext : ITenantContext
{
    public Guid ClinicId   { get; set; } = Guid.NewGuid();
    public Guid UserId     { get; set; } = Guid.NewGuid();
    public string Language { get; set; } = "en";
    public string TimeZone { get; set; } = "UTC";
}
