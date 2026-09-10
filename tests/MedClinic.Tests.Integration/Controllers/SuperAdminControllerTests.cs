using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MedClinic.Application.Features.SuperAdmin.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class SuperAdminControllerTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public SuperAdminControllerTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string superAdminToken, string regularDoctorToken, Guid clinicId)> SetupUsersAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = "SuperAdmin Test Clinic",
            Slug = $"sa-test-{Guid.NewGuid():N}"[..15],
            LifecycleStatus = ClinicLifecycleStatus.Active,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Clinics.Add(clinic);

        var superAdmin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"sa_{Guid.NewGuid():N}@medclinic.test",
            UserName = $"sa_{Guid.NewGuid():N}",
            FirstName = "Super",
            LastName = "Admin",
            IsActive = true
        };
        db.Users.Add(superAdmin);

        var doctorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doc_{Guid.NewGuid():N}@medclinic.test",
            UserName = $"doc_{Guid.NewGuid():N}",
            FirstName = "Doctor",
            LastName = "Standard",
            IsActive = true
        };
        db.Users.Add(doctorUser);

        db.ClinicMembers.Add(new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinic.Id,
            UserId = doctorUser.Id,
            Role = Roles.Doctor,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var saToken = jwt.GenerateAccessToken(superAdmin, [Roles.SuperAdmin]);
        var docToken = jwt.GenerateAccessTokenWithClinic(doctorUser, [Roles.Doctor], clinic.Id);

        return (saToken, docToken, clinic.Id);
    }

    [Fact]
    public async Task GetOverview_Returns401_WhenUnauthenticated()
    {
        var response = await _client.GetAsync("/api/v1/superadmin/overview");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOverview_Returns403_WhenCallerIsNotSuperAdmin()
    {
        var (_, docToken, _) = await SetupUsersAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/superadmin/overview");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", docToken);

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOverview_ReturnsPlatformKpis_WhenSuperAdmin()
    {
        var (saToken, _, _) = await SetupUsersAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/superadmin/overview");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", saToken);

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<SuperAdminOverviewDto>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.TotalClinics.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTenants_ReturnsPagedClinics_WhenSuperAdmin()
    {
        var (saToken, _, clinicId) = await SetupUsersAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/superadmin/tenants?page=1&pageSize=10");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", saToken);

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(clinicId.ToString());
    }

    [Fact]
    public async Task ProvisionTenant_CreatesClinic_AndReturns201()
    {
        var (saToken, _, _) = await SetupUsersAsync();
        var uniqueSlug = $"prov-{Guid.NewGuid():N}"[..12];

        var request = new ProvisionClinicRequest(
            Name: "Provisioned Health Corp",
            Slug: uniqueSlug,
            Description: "Enterprise Hospital branch",
            Email: $"info@{uniqueSlug}.com",
            Phone: "+1-800-555-0199",
            Address: "100 Medical Blvd",
            City: "Chicago",
            Country: "US",
            PlanCode: "pro",
            BillingCycle: BillingCycle.Annual,
            TrialDays: 14
        );

        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/superadmin/tenants/provision");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", saToken);
        req.Content = JsonContent.Create(request);

        var response = await _client.SendAsync(req);
        var err = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: $"API returned: {err}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var clinic = await db.Clinics.FirstOrDefaultAsync(c => c.Slug == uniqueSlug);
        clinic.Should().NotBeNull();
        clinic!.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Trial);
    }

    [Fact]
    public async Task SuspendAndReactivateTenant_UpdatesLifecycleSuccessfully()
    {
        var (saToken, _, clinicId) = await SetupUsersAsync();

        // 1. Suspend
        using var suspendReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/superadmin/tenants/{clinicId}/suspend");
        suspendReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", saToken);
        suspendReq.Content = JsonContent.Create(new SuspendClinicRequest("Terms of service breach"));

        var suspendRes = await _client.SendAsync(suspendReq);
        suspendRes.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var clinic = await db.Clinics.FindAsync(clinicId);
            clinic!.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Suspended);
            clinic.SuspensionReason.Should().Be("Terms of service breach");
        }

        // 2. Reactivate
        using var reactivateReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/superadmin/tenants/{clinicId}/reactivate");
        reactivateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", saToken);
        reactivateReq.Content = JsonContent.Create(new ReactivateClinicRequest("Terms accepted"));

        var reactivateRes = await _client.SendAsync(reactivateReq);
        reactivateRes.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var clinic = await db.Clinics.FindAsync(clinicId);
            clinic!.LifecycleStatus.Should().Be(ClinicLifecycleStatus.Active);
            clinic.SuspensionReason.Should().BeNull();
        }
    }
}
