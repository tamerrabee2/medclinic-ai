using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Services;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class ConsentsControllerTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ConsentsControllerTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid clinicAId, Guid clinicBId, Guid patientAId, Guid patientBId, string tokenUserA)> SeedConsentDataAsync()
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

        var doctorA = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doctor_{Guid.NewGuid():N}@clinica.com",
            UserName = $"doctor_{Guid.NewGuid():N}",
            FirstName = "Doctor",
            LastName = "Alpha",
            IsActive = true
        };
        db.Users.Add(doctorA);

        var memberA = new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            UserId = doctorA.Id,
            Role = Roles.Doctor,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        db.ClinicMembers.Add(memberA);

        var patientA = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            FirstName = "PatientA",
            LastName = "Consent",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1),
            CreatedAt = DateTime.UtcNow
        };

        var patientB = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicB.Id,
            FirstName = "PatientB",
            LastName = "Consent",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1995, 5, 5),
            CreatedAt = DateTime.UtcNow
        };

        db.Patients.AddRange(patientA, patientB);
        await db.SaveChangesAsync();

        var tokenA = jwtService.GenerateAccessTokenWithClinic(doctorA, [Roles.Doctor], clinicA.Id);

        return (clinicA.Id, clinicB.Id, patientA.Id, patientB.Id, tokenA);
    }

    [Fact]
    public void DependencyInjection_Resolves_IConsentService()
    {
        using var scope = _factory.Services.CreateScope();
        var consentService = scope.ServiceProvider.GetRequiredService<IConsentService>();

        consentService.Should().NotBeNull();
        consentService.Should().BeOfType<ConsentService>();
    }

    [Fact]
    public async Task ConsentsApi_Returns401_WhenUnauthenticated()
    {
        var response = await _client.GetAsync($"/api/v1/patients/{Guid.NewGuid()}/consents");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordConsent_ForAiAssistedCare_Succeeds_And_SetsActive()
    {
        var (clinicAId, _, patientAId, _, tokenA) = await SeedConsentDataAsync();

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        req.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        req.Content = JsonContent.Create(new
        {
            consentType = (int)ConsentType.AiAssistedCare,
            isGranted = true,
            expiresAt = DateTime.UtcNow.AddYears(1),
            notes = "Patient consented to Dr. AI assistance."
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.ConsentType.Should().Be(ConsentType.AiAssistedCare);
        body.IsGranted.Should().BeTrue();
        body.IsActive.Should().BeTrue();
        body.Notes.Should().Be("Patient consented to Dr. AI assistance.");
    }

    [Fact]
    public async Task HasActiveConsent_ReturnsTrue_WhenConsentIsGrantedAndNotExpired()
    {
        var (clinicAId, _, patientAId, _, tokenA) = await SeedConsentDataAsync();

        // 1. Record AI consent
        using (var postReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents"))
        {
            postReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            postReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            postReq.Content = JsonContent.Create(new
            {
                consentType = (int)ConsentType.AiAssistedCare,
                isGranted = true,
                expiresAt = DateTime.UtcNow.AddMonths(6)
            });
            var postRes = await _client.SendAsync(postReq);
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // 2. Query active status
        using var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientAId}/consents/active/{ConsentType.AiAssistedCare}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        getReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var res = await _client.SendAsync(getReq);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("hasActiveConsent").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task RevokeConsent_WithReason_Succeeds_And_SetsIsGrantedFalse()
    {
        var (clinicAId, _, patientAId, _, tokenA) = await SeedConsentDataAsync();

        // 1. Record consent
        Guid consentId;
        using (var postReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents"))
        {
            postReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            postReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            postReq.Content = JsonContent.Create(new
            {
                consentType = (int)ConsentType.AiAssistedCare,
                isGranted = true
            });
            var postRes = await _client.SendAsync(postReq);
            postRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await postRes.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
            consentId = created!.Id;
        }

        // 2. Revoke consent with reason
        using var revokeReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents/{consentId}/revoke");
        revokeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        revokeReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        revokeReq.Content = JsonContent.Create(new
        {
            reason = "Patient withdrew AI consent"
        });

        var revokeRes = await _client.SendAsync(revokeReq);
        revokeRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var revoked = await revokeRes.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
        revoked.Should().NotBeNull();
        revoked!.IsGranted.Should().BeFalse();
        revoked.IsActive.Should().BeFalse();
        revoked.Notes.Should().Contain("withdrew AI consent");

        // 3. Verify active check returns false
        using var checkReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientAId}/consents/active/{ConsentType.AiAssistedCare}");
        checkReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        checkReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var checkRes = await _client.SendAsync(checkReq);
        var checkJson = await checkRes.Content.ReadFromJsonAsync<JsonElement>();
        checkJson.GetProperty("hasActiveConsent").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task RevokeConsent_WithoutReason_Returns400BadRequest()
    {
        var (clinicAId, _, patientAId, _, tokenA) = await SeedConsentDataAsync();

        using var revokeReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents/{Guid.NewGuid()}/revoke");
        revokeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        revokeReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        revokeReq.Content = JsonContent.Create(new
        {
            reason = "   " // Empty/whitespace reason
        });

        var res = await _client.SendAsync(revokeReq);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordConsent_WithPastExpiry_Returns400BadRequest()
    {
        var (clinicAId, _, patientAId, _, tokenA) = await SeedConsentDataAsync();

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        req.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        req.Content = JsonContent.Create(new
        {
            consentType = (int)ConsentType.GeneralCare,
            isGranted = true,
            expiresAt = DateTime.UtcNow.AddMinutes(-5) // Past expiry
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ClinicA_CannotManage_ConsentOfPatientInClinicB_Returns404()
    {
        var (clinicAId, _, _, patientBId, tokenA) = await SeedConsentDataAsync();

        // 1. Try to record consent for Patient in Clinic B
        using (var postReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientBId}/consents"))
        {
            postReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            postReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            postReq.Content = JsonContent.Create(new
            {
                consentType = (int)ConsentType.DataSharing,
                isGranted = true
            });

            var postRes = await _client.SendAsync(postReq);
            postRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // 2. Try to view consent history for Patient in Clinic B
        using (var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientBId}/consents"))
        {
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            getReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

            var getRes = await _client.SendAsync(getReq);
            getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // 3. Try to check active consent for Patient in Clinic B
        using var activeReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientBId}/consents/active/{ConsentType.DataSharing}");
        activeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        activeReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var activeRes = await _client.SendAsync(activeReq);
        activeRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
