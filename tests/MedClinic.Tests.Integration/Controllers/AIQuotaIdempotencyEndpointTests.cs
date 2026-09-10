using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class AIQuotaIdempotencyEndpointTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public AIQuotaIdempotencyEndpointTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string token, Guid clinicId, Guid patientId, Guid labResultId, Guid imageId, Guid dicomStudyId)> SetupActiveAiContextAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var clinicId = Guid.NewGuid();
        var clinic = new Clinic
        {
            Id = clinicId,
            Name = "AI Quota Verification Clinic",
            Slug = $"ai-quota-{Guid.NewGuid():N}"[..15],
            LifecycleStatus = ClinicLifecycleStatus.Active,
            BillingStatus = BillingStatus.Current,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Clinics.Add(clinic);

        db.ClinicSubscriptions.Add(new ClinicSubscription
        {
            ClinicId = clinicId,
            Tier = SubscriptionTier.Pro,
            Status = SubscriptionStatus.Active,
            IsActive = true,
            StartDateUtc = DateTime.UtcNow.AddDays(-1),
            MonthlyAiRequestsLimitSnapshot = 100,
            FeaturesSnapshot = System.Text.Json.JsonSerializer.Serialize(new[] { FeatureKey.AiCopilot, FeatureKey.DicomPacs })
        });

        var doctorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doctor_ai_{Guid.NewGuid():N}@medclinic.test",
            UserName = $"doc_ai_{Guid.NewGuid():N}",
            FirstName = "Sami",
            LastName = "Ali",
            IsActive = true
        };
        db.Users.Add(doctorUser);

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = doctorUser.Id,
            Specialty = "General Practice",
            LicenseNumber = "DOC-AI-999"
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
            FirstName = "Layla",
            LastName = "Kareem",
            DateOfBirth = new DateTime(1990, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            NationalId = $"NAT-{Guid.NewGuid():N}"[..10],
            CreatedAt = DateTime.UtcNow
        };
        db.Patients.Add(patient);

        var labOrder = new LabOrder
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            TestName = "Complete Blood Count",
            OrderedAt = DateTime.UtcNow,
            Status = LabOrderStatus.Completed
        };
        db.LabOrders.Add(labOrder);

        var labResult = new LabResult
        {
            Id = Guid.NewGuid(),
            LabOrderId = labOrder.Id,
            Summary = "CBC result overview",
            IsAbnormal = true,
            ReportedAt = DateTime.UtcNow
        };
        db.LabResults.Add(labResult);

        var radStudy = new RadiologyStudy
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            StudyType = "Chest X-Ray",
            AccessionNumber = $"RAD-{Guid.NewGuid():N}"[..12],
            Status = RadiologyStudyStatus.Completed
        };
        db.RadiologyStudies.Add(radStudy);

        var radImage = new MedicalImage
        {
            Id = Guid.NewGuid(),
            RadiologyStudyId = radStudy.Id,
            FileName = "xray1.png",
            FileUrl = "uploads/xray1.png",
            FileSizeBytes = 1024,
            InstanceNumber = 1
        };
        db.MedicalImages.Add(radImage);

        db.ConsentRecords.Add(new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patient.Id,
            ConsentType = ConsentType.AiAssistedCare,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            GrantedByUserId = doctorUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });

        var dicomStudy = new DicomStudy
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            StudyInstanceUid = $"1.2.840.10008.{Guid.NewGuid():N}",
            Modality = "CT",
            StudyDescription = "Brain CT Scan",
            StudyDate = DateTime.UtcNow,
            InstanceCount = 1,
            Status = DicomStudyStatus.Available
        };
        db.DicomStudies.Add(dicomStudy);

        await db.SaveChangesAsync();

        var token = jwt.GenerateAccessTokenWithClinic(doctorUser, [Roles.Doctor], clinicId);
        return (token, clinicId, patient.Id, labResult.Id, radImage.Id, dicomStudy.Id);
    }

    [Fact]
    public async Task AIChat_WithSameIdempotencyKeyAndSamePayload_ReturnsSuccessIdempotently()
    {
        var (token, _, _, _, _, _) = await SetupActiveAiContextAsync();
        var key = $"idemp-chat-{Guid.NewGuid():N}";
        var payload = new SendMessageRequest(null, "Analyze these symptoms please", null, null, null);

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/chat");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Add("X-Idempotency-Key", key);
        req1.Content = JsonContent.Create(payload);

        var res1 = await _client.SendAsync(req1);
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Retry same key with same payload
        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/chat");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Add("X-Idempotency-Key", key);
        req2.Content = JsonContent.Create(payload);

        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AIChat_WithSameIdempotencyKeyAndMutatedPayload_Returns409IdempotencyKeyReused()
    {
        var (token, _, _, _, _, _) = await SetupActiveAiContextAsync();
        var key = $"idemp-chat-mut-{Guid.NewGuid():N}";
        var payload1 = new SendMessageRequest(null, "Initial query", null, null, null);
        var payload2 = new SendMessageRequest(null, "Completely different mutated query", null, null, null);

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/chat");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Add("X-Idempotency-Key", key);
        req1.Content = JsonContent.Create(payload1);

        var res1 = await _client.SendAsync(req1);
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Same key with different payload
        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/chat");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Add("X-Idempotency-Key", key);
        req2.Content = JsonContent.Create(payload2);

        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await res2.Content.ReadAsStringAsync();
        body.Should().Contain("idempotency_key_reused");
    }

    [Fact]
    public async Task AnalyzeLab_WithSameIdempotencyKeyAndMutatedPayload_Returns409IdempotencyKeyReused()
    {
        var (token, clinicId, _, labResultId, _, _) = await SetupActiveAiContextAsync();
        var key = $"idemp-lab-mut-{Guid.NewGuid():N}";
        var payload1 = new AnalyzeLabRequest(labResultId, CompareWithPrevious: true);
        var payload2 = new AnalyzeLabRequest(labResultId, CompareWithPrevious: false);

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/analyze/lab");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req1.Headers.Add("X-Idempotency-Key", key);
        req1.Content = JsonContent.Create(payload1);

        var res1 = await _client.SendAsync(req1);
        var res1Body = await res1.Content.ReadAsStringAsync();
        res1.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected OK but got {res1.StatusCode} with body: {res1Body}");

        // Reused key with mutated payload
        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/analyze/lab");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req2.Headers.Add("X-Idempotency-Key", key);
        req2.Content = JsonContent.Create(payload2);

        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await res2.Content.ReadAsStringAsync();
        body.Should().Contain("idempotency_key_reused");
    }

    [Fact]
    public async Task GeneratePatientSummary_WithSameIdempotencyKeyAndMutatedPayload_Returns409IdempotencyKeyReused()
    {
        var (token, clinicId, patientId, _, _, _) = await SetupActiveAiContextAsync();
        var key = $"idemp-summary-mut-{Guid.NewGuid():N}";
        var payload1 = new GeneratePatientSummaryRequest(patientId, IncludeLabTrends: true);
        var payload2 = new GeneratePatientSummaryRequest(patientId, IncludeLabTrends: false);

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/analyze/patient-summary");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req1.Headers.Add("X-Idempotency-Key", key);
        req1.Content = JsonContent.Create(payload1);

        var res1 = await _client.SendAsync(req1);
        var res1Body = await res1.Content.ReadAsStringAsync();
        res1.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected OK but got {res1.StatusCode} with body: {res1Body}");

        // Reused key with mutated payload
        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/analyze/patient-summary");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req2.Headers.Add("X-Idempotency-Key", key);
        req2.Content = JsonContent.Create(payload2);

        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await res2.Content.ReadAsStringAsync();
        body.Should().Contain("idempotency_key_reused");
    }

    [Fact]
    public async Task AnalyzeImage_WithSameIdempotencyKeyAndMutatedPayload_Returns409IdempotencyKeyReused()
    {
        var (token, clinicId, _, _, imageId, _) = await SetupActiveAiContextAsync();
        var key = $"idemp-img-mut-{Guid.NewGuid():N}";
        var payload1 = new AnalyzeImageRequest(imageId, ClinicalContext: "Check for pneumonia");
        var payload2 = new AnalyzeImageRequest(imageId, ClinicalContext: "Check for rib fractures");

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/analyze/image");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req1.Headers.Add("X-Idempotency-Key", key);
        req1.Content = JsonContent.Create(payload1);

        var res1 = await _client.SendAsync(req1);
        var res1Body = await res1.Content.ReadAsStringAsync();
        res1.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected OK but got {res1.StatusCode} with body: {res1Body}");

        // Reused key with mutated payload
        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ai/analyze/image");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req2.Headers.Add("X-Idempotency-Key", key);
        req2.Content = JsonContent.Create(payload2);

        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await res2.Content.ReadAsStringAsync();
        body.Should().Contain("idempotency_key_reused");
    }

    [Fact]
    public async Task DicomAnalyze_WithSameIdempotencyKeyAndDifferentStudyId_Returns409IdempotencyKeyReused()
    {
        var (token, clinicId, patientId, _, _, dicomStudyId) = await SetupActiveAiContextAsync();
        var secondStudyId = Guid.NewGuid();

        // Seed a second study
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.DicomStudies.Add(new DicomStudy
            {
                Id = secondStudyId,
                ClinicId = clinicId,
                PatientId = patientId,
                StudyInstanceUid = $"1.2.840.10008.{Guid.NewGuid():N}",
                Modality = "CT",
                StudyDescription = "Spine CT",
                StudyDate = DateTime.UtcNow,
                InstanceCount = 1,
                Status = DicomStudyStatus.Available
            });
            await db.SaveChangesAsync();
        }

        var key = $"idemp-dicom-mut-{Guid.NewGuid():N}";

        using var req1 = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientId}/dicom/{dicomStudyId}/analyze");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req1.Headers.Add("X-Idempotency-Key", key);

        var res1 = await _client.SendAsync(req1);
        var res1Body = await res1.Content.ReadAsStringAsync();
        res1.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected OK but received {res1.StatusCode} with body: {res1Body}");

        // Same idempotency key with different study ID
        using var req2 = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/patients/{patientId}/dicom/{secondStudyId}/analyze");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req2.Headers.Add("X-Idempotency-Key", key);

        var res2 = await _client.SendAsync(req2);
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await res2.Content.ReadAsStringAsync();
        body.Should().Contain("idempotency_key_reused");
    }
}
