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
using Microsoft.EntityFrameworkCore;
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

    private async Task<(Guid clinicAId, Guid clinicBId, Guid patientAId, Guid patientBId, string tokenDoctorA, string tokenNurseA, string tokenReceptionistA)> SeedConsentDataAsync()
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

        // Doctor A
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
        db.ClinicMembers.Add(new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            UserId = doctorA.Id,
            Role = Roles.Doctor,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        // Nurse A
        var nurseA = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"nurse_{Guid.NewGuid():N}@clinica.com",
            UserName = $"nurse_{Guid.NewGuid():N}",
            FirstName = "Nurse",
            LastName = "Beta",
            IsActive = true
        };
        db.Users.Add(nurseA);
        db.ClinicMembers.Add(new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            UserId = nurseA.Id,
            Role = Roles.Nurse,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

        // Receptionist A
        var receptionistA = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"receptionist_{Guid.NewGuid():N}@clinica.com",
            UserName = $"receptionist_{Guid.NewGuid():N}",
            FirstName = "Receptionist",
            LastName = "Gamma",
            IsActive = true
        };
        db.Users.Add(receptionistA);
        db.ClinicMembers.Add(new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicA.Id,
            UserId = receptionistA.Id,
            Role = Roles.Receptionist,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });

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

        var tokenDoctorA = jwtService.GenerateAccessTokenWithClinic(doctorA, [Roles.Doctor], clinicA.Id);
        var tokenNurseA = jwtService.GenerateAccessTokenWithClinic(nurseA, [Roles.Nurse], clinicA.Id);
        var tokenReceptionistA = jwtService.GenerateAccessTokenWithClinic(receptionistA, [Roles.Receptionist], clinicA.Id);

        return (clinicA.Id, clinicB.Id, patientA.Id, patientB.Id, tokenDoctorA, tokenNurseA, tokenReceptionistA);
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
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

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
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

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
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

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
    public async Task RevokeConsent_CannotBeReRevoked_Returns400BadRequest()
    {
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

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
            var created = await postRes.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
            consentId = created!.Id;
        }

        // 2. Revoke first time
        using (var revokeReq1 = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents/{consentId}/revoke"))
        {
            revokeReq1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            revokeReq1.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            revokeReq1.Content = JsonContent.Create(new { reason = "Initial revocation" });
            var res1 = await _client.SendAsync(revokeReq1);
            res1.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // 3. Attempt to revoke second time -> Must be rejected by immutable safeguard
        using var revokeReq2 = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents/{consentId}/revoke");
        revokeReq2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        revokeReq2.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        revokeReq2.Content = JsonContent.Create(new { reason = "Second revocation attempt" });

        var res2 = await _client.SendAsync(revokeReq2);
        res2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuditTrail_CapturesImmutableGrantedAndRevokedEvents()
    {
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

        // 1. Record consent
        Guid consentId;
        using (var postReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents"))
        {
            postReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            postReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            postReq.Content = JsonContent.Create(new
            {
                consentType = (int)ConsentType.DataSharing,
                isGranted = true,
                notes = "Consent for external specialist sharing"
            });
            var postRes = await _client.SendAsync(postReq);
            var created = await postRes.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
            consentId = created!.Id;
        }

        // 2. Revoke consent
        using (var revokeReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents/{consentId}/revoke"))
        {
            revokeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            revokeReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            revokeReq.Content = JsonContent.Create(new { reason = "Patient requested revocation" });
            var revokeRes = await _client.SendAsync(revokeReq);
            revokeRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // 3. Query audit trail
        using var auditReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientAId}/consents/audit?consentId={consentId}");
        auditReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        auditReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var auditRes = await _client.SendAsync(auditReq);
        auditRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await auditRes.Content.ReadFromJsonAsync<List<ConsentAuditEventDto>>(JsonOptions);
        events.Should().NotBeNull();
        events!.Count.Should().Be(2);

        events.Should().Contain(e => e.EventType == ConsentAuditEventType.Revoked && e.Reason == "Patient requested revocation");
        events.Should().Contain(e => e.EventType == ConsentAuditEventType.Granted);
    }

    [Fact]
    public async Task RBAC_Receptionist_CannotRecordOrRevokeConsent_Returns403()
    {
        var (clinicAId, _, patientAId, _, _, _, tokenReceptionist) = await SeedConsentDataAsync();

        // 1. Receptionist tries to Record consent (Requires PatientConsents.Manage)
        using (var postReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents"))
        {
            postReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenReceptionist);
            postReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            postReq.Content = JsonContent.Create(new
            {
                consentType = (int)ConsentType.GeneralCare,
                isGranted = true
            });

            var postRes = await _client.SendAsync(postReq);
            postRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // 2. Receptionist CAN read consent history (Has PatientConsents.View)
        using (var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientAId}/consents"))
        {
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenReceptionist);
            getReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

            var getRes = await _client.SendAsync(getReq);
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task RBAC_Nurse_CanRecord_ButCannotRevokeConsent_Returns403()
    {
        var (clinicAId, _, patientAId, _, tokenDoctor, tokenNurse, _) = await SeedConsentDataAsync();

        // 1. Doctor records consent
        Guid consentId;
        using (var postReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents"))
        {
            postReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenDoctor);
            postReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            postReq.Content = JsonContent.Create(new
            {
                consentType = (int)ConsentType.GeneralCare,
                isGranted = true
            });

            var postRes = await _client.SendAsync(postReq);
            var created = await postRes.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
            consentId = created!.Id;
        }

        // 2. Nurse tries to Revoke consent (Requires PatientConsents.Revoke)
        using var revokeReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents/{consentId}/revoke");
        revokeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenNurse);
        revokeReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        revokeReq.Content = JsonContent.Create(new { reason = "Nurse trying to revoke" });

        var revokeRes = await _client.SendAsync(revokeReq);
        revokeRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AiConsentGuard_BlocksClinicalAI_WhenConsentNotGranted()
    {
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

        // 1. Try to invoke Clinical AI without active consent -> Should fail with 400
        using (var aiReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/clinical-ai/risk-score"))
        {
            aiReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            aiReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            aiReq.Content = JsonContent.Create(new
            {
                age = 45,
                vitals = new Dictionary<string, string> { ["BP"] = "120/80" },
                factors = new Dictionary<string, string> { ["Smoker"] = "No" }
            });

            var aiRes = await _client.SendAsync(aiReq);
            aiRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var json = await aiRes.Content.ReadFromJsonAsync<JsonElement>();
            json.GetProperty("type").GetString().Should().Be("consent_required");
            json.GetProperty("status").GetInt32().Should().Be(403);
            json.GetProperty("consentType").GetString().Should().Be(nameof(ConsentType.AiAssistedCare));
        }

        // 2. Grant AI consent
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
        }

        // 3. Invoke Clinical AI again -> Should succeed (200 OK)
        using (var aiReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/clinical-ai/risk-score"))
        {
            aiReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            aiReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
            aiReq.Content = JsonContent.Create(new
            {
                age = 45,
                vitals = new Dictionary<string, string> { ["BP"] = "120/80" },
                factors = new Dictionary<string, string> { ["Smoker"] = "No" }
            });

            var aiRes = await _client.SendAsync(aiReq);
            aiRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task RevokeConsent_WithoutReason_Returns400BadRequest()
    {
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

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
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

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
        var (clinicAId, _, _, patientBId, tokenA, _, _) = await SeedConsentDataAsync();

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

    [Fact]
    public async Task RecordConsent_IgnoresAnyClientAttemptToSpoofWitnessId()
    {
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

        var spoofedWitnessId = Guid.NewGuid();

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientAId}/consents");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        req.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        req.Content = JsonContent.Create(new
        {
            consentType = (int)ConsentType.GeneralCare,
            isGranted = true,
            notes = "Testing witness spoofing prevention",
            witnessUserId = spoofedWitnessId // Attacker tries to forge witness
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await res.Content.ReadFromJsonAsync<ConsentDto>(JsonOptions);
        body.Should().NotBeNull();

        // The system must NEVER accept client-forged witness IDs
        body!.WitnessUserId.Should().NotBe(spoofedWitnessId);

        // It must securely match the authenticated caller's identity
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var doctorMember = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstAsync(
            db.ClinicMembers, m => m.ClinicId == clinicAId && m.Role == Roles.Doctor);

        body.WitnessUserId.Should().Be(doctorMember.UserId);
        body.GrantedByUserId.Should().Be(doctorMember.UserId);
    }

    [Fact]
    public async Task HasActiveConsent_ReturnsFalse_WhenConsentIsExpiredInDatabase()
    {
        var (clinicAId, _, patientAId, _, tokenA, _, _) = await SeedConsentDataAsync();

        // 1. Seed an expired consent record directly into database
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var expiredConsent = new ConsentRecord
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                PatientId = patientAId,
                ConsentType = ConsentType.Marketing,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow.AddMonths(-6),
                ExpiresAt = DateTime.UtcNow.AddDays(-2), // expired 2 days ago
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };
            db.ConsentRecords.Add(expiredConsent);
            await db.SaveChangesAsync();
        }

        // 2. Query active status
        using var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientAId}/consents/active/{ConsentType.Marketing}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        getReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var res = await _client.SendAsync(getReq);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await res.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("hasActiveConsent").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Compliance_HardDelete_OnConsentRecordOrAuditEvents_IsProhibited()
    {
        var (clinicAId, _, patientAId, _, _, _, _) = await SeedConsentDataAsync();

        // 1. Verify Restrict behavior prevents deleting ConsentRecord when ConsentAuditEvent exists
        using (var scope1 = _factory.Services.CreateScope())
        {
            var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var consent = new ConsentRecord
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                PatientId = patientAId,
                ConsentType = ConsentType.GeneralCare,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            db1.ConsentRecords.Add(consent);

            var audit = new ConsentAuditEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                ConsentRecordId = consent.Id,
                PatientId = patientAId,
                EventType = ConsentAuditEventType.Granted,
                ConsentType = ConsentType.GeneralCare,
                Timestamp = DateTime.UtcNow
            };
            db1.ConsentAuditEvents.Add(audit);
            await db1.SaveChangesAsync();

            var removeConsent = () => db1.ConsentRecords.Remove(consent);
            removeConsent.Should().Throw<InvalidOperationException>()
                .WithMessage("*association between entity types 'ConsentRecord' and 'ConsentAuditEvent' has been severed*");
        }

        // 2. Direct hard delete on standalone ConsentRecord is blocked by compliance safeguard in SaveChangesAsync
        using (var scope2 = _factory.Services.CreateScope())
        {
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var standaloneConsent = new ConsentRecord
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                PatientId = patientAId,
                ConsentType = ConsentType.Marketing,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            db2.ConsentRecords.Add(standaloneConsent);
            await db2.SaveChangesAsync();

            db2.ConsentRecords.Remove(standaloneConsent);
            var saveStandalone = async () => await db2.SaveChangesAsync();
            await saveStandalone.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Hard deletion of consent compliance records is prohibited*");
        }

        // 3. Attempting hard delete on ConsentAuditEvent is blocked by compliance safeguard in SaveChangesAsync
        using (var scope3 = _factory.Services.CreateScope())
        {
            var db3 = scope3.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var standaloneAudit = new ConsentAuditEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                ConsentRecordId = Guid.NewGuid(),
                PatientId = patientAId,
                EventType = ConsentAuditEventType.Granted,
                ConsentType = ConsentType.Research,
                Timestamp = DateTime.UtcNow
            };
            db3.ConsentAuditEvents.Add(standaloneAudit);
            await db3.SaveChangesAsync();

            db3.ConsentAuditEvents.Remove(standaloneAudit);
            var saveAudit = async () => await db3.SaveChangesAsync();
            await saveAudit.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Hard deletion of consent compliance records is prohibited*");
        }
    }

    [Fact]
    public async Task Compliance_LegalHold_PreventsSoftDeleteOrArchival()
    {
        var (clinicAId, _, patientAId, _, _, _, _) = await SeedConsentDataAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var consent = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicAId,
            PatientId = patientAId,
            ConsentType = ConsentType.Research,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsLegalHold = true,
            LegalHoldReason = "Pending compliance audit subpoena"
        };
        db.ConsentRecords.Add(consent);
        await db.SaveChangesAsync();

        // Attempting soft-delete on record with active LegalHold must be rejected
        consent.IsDeleted = true;
        var act = async () => await db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*subject to an active legal hold and cannot be deleted or archived*");
    }

    [Fact]
    public async Task AuditExplorer_DateRangeValidation_ReturnsBadRequest_WhenFromIsAfterTo()
    {
        var (clinicAId, _, _, _, tokenDoctorA, _, _) = await SeedConsentDataAsync();

        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/audit/explorer/consents?from=2026-09-10T00:00:00Z&to=2026-09-01T00:00:00Z");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenDoctorA);
        req.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuditExplorer_CrossTenantIsolation_CannotAccessOtherClinicAudits()
    {
        var (clinicAId, clinicBId, patientAId, patientBId, tokenDoctorA, _, _) = await SeedConsentDataAsync();

        // 1. Seed audit event in Clinic B
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var consentB = new ConsentRecord
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicBId,
                PatientId = patientBId,
                ConsentType = ConsentType.GeneralCare,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            db.ConsentRecords.Add(consentB);
            db.ConsentAuditEvents.Add(new ConsentAuditEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicBId,
                ConsentRecordId = consentB.Id,
                PatientId = patientBId,
                EventType = ConsentAuditEventType.Granted,
                ConsentType = ConsentType.GeneralCare,
                Reason = "Secret Clinic B Consent Audit Event",
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 2. Doctor A from Clinic A queries Audit Explorer
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/audit/explorer/consents");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenDoctorA);
        req.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().NotContain("Secret Clinic B Consent Audit Event");
    }

    [Fact]
    public async Task AuditExplorer_LegalHold_GeneratesImmutableConsentAuditEvent()
    {
        var (clinicAId, _, patientAId, _, tokenDoctorA, _, _) = await SeedConsentDataAsync();

        Guid consentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var consent = new ConsentRecord
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                PatientId = patientAId,
                ConsentType = ConsentType.AiAssistedCare,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            db.ConsentRecords.Add(consent);
            await db.SaveChangesAsync();
            consentId = consent.Id;
        }

        // 1. Apply legal hold via AuditExplorerController (using Admin/SuperAdmin role)
        using var adminScope = _factory.Services.CreateScope();
        var jwt = adminScope.ServiceProvider.GetRequiredService<IJwtService>();
        var dbCtx = adminScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"admin_{Guid.NewGuid():N}@clinic.com",
            UserName = $"admin_{Guid.NewGuid():N}",
            FirstName = "Compliance",
            LastName = "Admin",
            IsActive = true
        };
        dbCtx.Users.Add(adminUser);
        dbCtx.ClinicMembers.Add(new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicAId,
            UserId = adminUser.Id,
            Role = Roles.ClinicAdmin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });
        await dbCtx.SaveChangesAsync();
        var adminToken = jwt.GenerateAccessTokenWithClinic(adminUser, [Roles.ClinicAdmin], clinicAId);

        using var holdReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/audit/explorer/legal-hold");
        holdReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        holdReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        holdReq.Content = JsonContent.Create(new
        {
            consentRecordId = consentId,
            isLegalHold = true,
            reason = "Judicial investigation subpoena #8891"
        });

        var holdRes = await _client.SendAsync(holdReq);
        holdRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Verify an immutable ConsentAuditEvent was automatically logged
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditEntry = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            verifyDb.ConsentAuditEvents.IgnoreQueryFilters(),
            a => a.ConsentRecordId == consentId && a.Reason == "Judicial investigation subpoena #8891");

        auditEntry.Should().NotBeNull();
        auditEntry!.Details.Should().Contain("Active legal hold applied");
    }

    [Fact]
    public async Task AuditExplorer_ExportCsv_MasksPiiWhenRequested()
    {
        var (clinicAId, _, patientAId, _, tokenDoctorA, _, _) = await SeedConsentDataAsync();

        // 1. Seed a consent audit event
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var consent = new ConsentRecord
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                PatientId = patientAId,
                ConsentType = ConsentType.DataSharing,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            db.ConsentRecords.Add(consent);
            db.ConsentAuditEvents.Add(new ConsentAuditEvent
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicAId,
                ConsentRecordId = consent.Id,
                PatientId = patientAId,
                EventType = ConsentAuditEventType.Granted,
                ConsentType = ConsentType.DataSharing,
                IpAddress = "192.168.1.55",
                Reason = "Clinical data sharing for second opinion",
                Timestamp = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // 2. Request export with maskPii = true
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/audit/explorer/consents/export?maskPii=true");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenDoctorA);
        req.Headers.Add("X-Clinic-Id", clinicAId.ToString());

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");

        var csv = await res.Content.ReadAsStringAsync();
        csv.Should().Contain("EventId,TimestampUtc,PatientIdentifier");
        // IP address must be masked
        csv.Should().Contain("192.168.***.***");
    }

    [Fact]
    public async Task AiConsentGuard_BlocksAiChat_WhenConsentIsMissingOrRevoked()
    {
        var (clinicAId, _, patientAId, _, tokenDoctorA, _, _) = await SeedConsentDataAsync();

        // Attempting AI chat with a patient context lacking active consent must return 403 Forbidden with consent_required
        using var chatReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/chat");
        chatReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenDoctorA);
        chatReq.Headers.Add("X-Clinic-Id", clinicAId.ToString());
        chatReq.Content = JsonContent.Create(new
        {
            message = "Please evaluate diabetes risk for this patient",
            patientContextId = patientAId
        });

        var chatRes = await _client.SendAsync(chatReq);
        chatRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await chatRes.Content.ReadAsStringAsync();
        body.Should().Contain("consent_required");
    }
}

