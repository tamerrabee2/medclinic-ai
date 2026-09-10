using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class TenantSuspensionMatrixEndpointTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public TenantSuspensionMatrixEndpointTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string token, Guid clinicId, Guid patientId, Guid doctorId, Guid visitId, Guid consentId)> SetupSuspendedClinicContextAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var clinicId = Guid.NewGuid();
        var clinic = new Clinic
        {
            Id = clinicId,
            Name = "Suspended Regional Clinic",
            Slug = $"susp-{Guid.NewGuid():N}"[..15],
            LifecycleStatus = ClinicLifecycleStatus.Suspended,
            SuspensionReason = "Account delinquent. Operational mutations are restricted.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Clinics.Add(clinic);

        var doctorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doc_susp_{Guid.NewGuid():N}@medclinic.test",
            UserName = $"doc_susp_{Guid.NewGuid():N}",
            FirstName = "Tarek",
            LastName = "Mahmoud",
            IsActive = true
        };
        db.Users.Add(doctorUser);

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = doctorUser.Id,
            Specialty = "Internal Medicine",
            LicenseNumber = "DOC-SUSP-123"
        };
        db.Doctors.Add(doctor);

        db.ClinicMembers.Add(new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = doctorUser.Id,
            Role = Roles.Doctor,
            IsActive = true
        });

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            FirstName = "Omar",
            LastName = "Hassan",
            DateOfBirth = new DateTime(1985, 4, 12, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            NationalId = $"NAT-{Guid.NewGuid():N}"[..10],
            CreatedAt = DateTime.UtcNow
        };
        db.Patients.Add(patient);

        var visit = new Visit
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            VisitDate = DateTime.UtcNow.AddDays(-2),
            Status = VisitStatus.Completed,
            ChiefComplaint = "Chronic cough and fatigue",
            Diagnosis = "Mild acute bronchitis",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        db.Visits.Add(visit);

        var consent = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patient.Id,
            ConsentType = ConsentType.GeneralCare,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow.AddDays(-5),
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            GrantedByUserId = doctorUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        db.ConsentRecords.Add(consent);

        await db.SaveChangesAsync();

        var token = jwt.GenerateAccessTokenWithClinic(doctorUser, [Roles.Doctor], clinicId);
        return (token, clinicId, patient.Id, doctor.Id, visit.Id, consent.Id);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BLOCKED ENDPOINTS (Must return 403 Forbidden with clinic_suspended)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAppointment_WhenClinicSuspended_Returns403ClinicSuspended()
    {
        var (token, _, patientId, doctorId, _, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/appointments");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = DateTime.UtcNow.AddDays(2),
            DurationMinutes = 30,
            Type = "Consultation",
            Notes = "Should be blocked due to clinic suspension"
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("clinic_suspended");
    }

    [Fact]
    public async Task CreateVisit_WhenClinicSuspended_Returns403ClinicSuspended()
    {
        var (token, _, patientId, doctorId, _, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/visits");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ChiefComplaint = "Headache",
            Symptoms = "Fever"
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("clinic_suspended");
    }

    [Fact]
    public async Task CreatePrescription_WhenClinicSuspended_Returns403ClinicSuspended()
    {
        var (token, _, patientId, doctorId, visitId, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/prescriptions");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new
        {
            PatientId = patientId,
            DoctorId = doctorId,
            VisitId = visitId,
            Items = new[]
            {
                new { MedicineName = "Amoxicillin 500mg", Dosage = "500mg", Frequency = "TID", DurationDays = 7, Route = "Oral", Quantity = 21 }
            }
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("clinic_suspended");
    }

    [Fact]
    public async Task AIChat_WhenClinicSuspended_Returns403ClinicSuspended()
    {
        var (token, _, _, _, _, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/chat");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new
        {
            Message = "Suggest treatment for hypertension"
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("clinic_suspended");
    }

    [Fact]
    public async Task CreateLabOrder_WhenClinicSuspended_Returns403ClinicSuspended()
    {
        var (token, _, patientId, doctorId, visitId, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/lab-orders");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new
        {
            PatientId = patientId,
            DoctorId = doctorId,
            VisitId = visitId,
            TestName = "Complete Blood Count (CBC)",
            IsUrgent = false
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("clinic_suspended");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRESERVED ENDPOINTS (Must remain 200 OK under suspension for compliance/patient rights)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReadPatientRecord_WhenClinicSuspended_Returns200Ok()
    {
        var (token, _, patientId, _, _, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/patients/{patientId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("Omar");
        body.Should().Contain("Hassan");
    }

    [Fact]
    public async Task ReadVisitRecord_WhenClinicSuspended_Returns200Ok()
    {
        var (token, _, _, _, visitId, _) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/visits/{visitId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("Chronic cough and fatigue");
    }

    [Fact]
    public async Task RevokePatientConsent_WhenClinicSuspended_Returns200Ok_PreservingPatientRights()
    {
        var (token, _, patientId, _, _, consentId) = await SetupSuspendedClinicContextAsync();
        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientId}/consents/{consentId}/revoke");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(new
        {
            Reason = "Patient requested revocation under GDPR/HIPAA compliance mandate"
        });

        var res = await _client.SendAsync(req);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("Revoked");
    }
}
