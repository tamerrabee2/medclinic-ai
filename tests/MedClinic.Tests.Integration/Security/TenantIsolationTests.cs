using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedClinic.Tests.Integration.Security;

public class TenantIsolationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public TenantIsolationTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid clinicAId, Guid clinicBId, Guid patientAId, Guid patientBId, string tokenUserA)> SeedTenantDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var clinicA = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = $"Clinic A {Guid.NewGuid():N}",
            Slug = $"ca-{Guid.NewGuid():N}"[..10],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var clinicB = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = $"Clinic B {Guid.NewGuid():N}",
            Slug = $"cb-{Guid.NewGuid():N}"[..10],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Clinics.AddRange(clinicA, clinicB);

        var userA = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doctor_{Guid.NewGuid():N}@clinica.com",
            UserName = $"doctor_{Guid.NewGuid():N}",
            FirstName = "Doctor",
            LastName = "Alpha",
            IsActive = true
        };
        db.Users.Add(userA);

        var memberA = new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            UserId = userA.Id,
            Role = Roles.Doctor,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        db.ClinicMembers.Add(memberA);

        var patientA = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            FirstName = "Patient",
            LastName = "InClinicA",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1),
            CreatedAt = DateTime.UtcNow
        };

        var patientB = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicB.Id,
            FirstName = "Patient",
            LastName = "InClinicB",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1992, 2, 2),
            CreatedAt = DateTime.UtcNow
        };

        db.Patients.AddRange(patientA, patientB);
        await db.SaveChangesAsync();

        var tokenUserA = jwtService.GenerateAccessTokenWithClinic(userA, [Roles.Doctor], clinicA.Id);

        return (clinicA.Id, clinicB.Id, patientA.Id, patientB.Id, tokenUserA);
    }

    [Fact]
    public async Task User_CannotAccess_OtherClinic_Returns403()
    {
        // Arrange
        var (clinicAId, clinicBId, _, _, tokenUserA) = await SeedTenantDataAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/patients");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUserA);
        request.Headers.Add("X-Clinic-Id", clinicBId.ToString()); // Attempting to access Clinic B

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task QueryPatients_OnlyReturnsPatientsForOwnClinic()
    {
        // Arrange
        var (clinicAId, clinicBId, patientAId, patientBId, tokenUserA) = await SeedTenantDataAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/patients");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUserA);
        request.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(patientAId.ToString());
        content.Should().NotContain(patientBId.ToString());
    }

    [Fact]
    public async Task GetPatientById_FromDifferentClinic_Returns404()
    {
        // Arrange
        var (clinicAId, _, _, patientBId, tokenUserA) = await SeedTenantDataAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientBId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUserA);
        request.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
