using FluentAssertions;
using MedClinic.API.Controllers;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class RbacAuthorizationTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public RbacAuthorizationTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string receptionistToken, string nurseToken, string doctorToken, Guid clinicId)> SetupRolesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = "RBAC Test Clinic",
            Slug = "rbac-test-clinic-" + Guid.NewGuid().ToString("N")[..6],
            IsActive = true
        };
        db.Clinics.Add(clinic);

        var receptionistUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"reception-{Guid.NewGuid():N}@test.com",
            UserName = $"reception-{Guid.NewGuid():N}@test.com",
            FirstName = "Sara",
            LastName = "Receptionist",
            SecurityStamp = Guid.NewGuid().ToString(),
            IsActive = true
        };
        db.Users.Add(receptionistUser);

        var nurseUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"nurse-{Guid.NewGuid():N}@test.com",
            UserName = $"nurse-{Guid.NewGuid():N}@test.com",
            FirstName = "Mona",
            LastName = "Nurse",
            SecurityStamp = Guid.NewGuid().ToString(),
            IsActive = true
        };
        db.Users.Add(nurseUser);

        var doctorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doctor-{Guid.NewGuid():N}@test.com",
            UserName = $"doctor-{Guid.NewGuid():N}@test.com",
            FirstName = "Dr. Tariq",
            LastName = "Doctor",
            SecurityStamp = Guid.NewGuid().ToString(),
            IsActive = true
        };
        db.Users.Add(doctorUser);

        await db.SaveChangesAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>>();
        if (!await roleManager.RoleExistsAsync(Roles.Doctor))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>(Roles.Doctor));
        await userManager.AddToRoleAsync(doctorUser, Roles.Doctor);

        var recToken = jwt.GenerateAccessTokenWithClinic(receptionistUser, [Roles.Receptionist], clinic.Id);
        var nurToken = jwt.GenerateAccessTokenWithClinic(nurseUser, [Roles.Nurse], clinic.Id);
        var docToken = jwt.GenerateAccessTokenWithClinic(doctorUser, [Roles.Doctor], clinic.Id);

        return (recToken, nurToken, docToken, clinic.Id);
    }

    [Fact]
    public async Task GetPrescriptions_Returns401_WhenUnauthenticated()
    {
        var response = await _client.GetAsync("/api/v1/prescriptions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPrescriptions_Returns403_WhenCallerIsReceptionist()
    {
        var (recToken, _, _, _) = await SetupRolesAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/prescriptions");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", recToken);

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SignPrescription_Returns403_WhenCallerIsNurse()
    {
        var (_, nurToken, _, _) = await SetupRolesAsync();
        var fakeId = Guid.NewGuid();
        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/prescriptions/{fakeId}/sign");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", nurToken);

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMe_ReturnsUserPermissions_WhenAuthenticated()
    {
        var (_, _, docToken, _) = await SetupRolesAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", docToken);

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        content.Should().NotBeNull();
        content!.Data.Should().NotBeNull();
        content.Data!.Roles.Should().Contain(Roles.Doctor);
        content.Data.Permissions.Should().Contain(Permissions.PrescriptionsRead);
        content.Data.Permissions.Should().Contain(Permissions.PrescriptionsSign);
        content.Data.Permissions.Should().Contain(Permissions.AIAssist);
        content.Data.Permissions.Should().NotContain(Permissions.UsersManage);
    }
}
