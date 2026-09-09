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

public class AiAuditsControllerTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public AiAuditsControllerTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid clinicId, Guid auditId, string doctorToken)> SeedAuditDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtService>();

        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = "AI Test Clinic",
            Slug = $"ai-test-{Guid.NewGuid():N}"[..15],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Clinics.Add(clinic);

        var doctorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"doctor_{Guid.NewGuid():N}@medclinic.test",
            UserName = $"doc_{Guid.NewGuid():N}",
            FirstName = "Doctor",
            LastName = "Reviewer",
            IsActive = true
        };
        db.Users.Add(doctorUser);

        var member = new ClinicMember
        {
            Id = Guid.NewGuid(),
            ClinicId = clinic.Id,
            UserId = doctorUser.Id,
            Role = Roles.Doctor,
            IsActive = true
        };
        db.ClinicMembers.Add(member);

        var audit = new AiDecisionAudit
        {
            Id = Guid.NewGuid(),
            ClinicId = clinic.Id,
            DoctorId = doctorUser.Id,
            Capability = "LabAnalysis",
            ProviderName = "Mock",
            ModelVersion = "1.0",
            InputHash = "hash123",
            OutputHash = "hash456",
            ReviewStatus = AiReviewStatus.PendingReview,
            CreatedAt = DateTime.UtcNow
        };
        db.AiDecisionAudits.Add(audit);
        await db.SaveChangesAsync();

        var token = jwt.GenerateAccessTokenWithClinic(doctorUser, [Roles.Doctor], clinic.Id);
        return (clinic.Id, audit.Id, token);
    }

    [Fact]
    public async Task GetAudits_Returns401_WhenUnauthenticated()
    {
        var response = await _client.GetAsync("/api/v1/ai/audits");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReviewAudit_Rejected_WithoutOverrideReason_Returns400()
    {
        var (clinicId, auditId, token) = await SeedAuditDataAsync();

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ai/audits/{auditId}/review");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req.Content = JsonContent.Create(new
        {
            status = (int)AiReviewStatus.Rejected,
            overrideReason = (string?)null // Missing required override reason
        });

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReviewAudit_Modified_WithOverrideReason_Returns200()
    {
        var (clinicId, auditId, token) = await SeedAuditDataAsync();

        using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/ai/audits/{auditId}/review");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.Add("X-Clinic-Id", clinicId.ToString());
        req.Content = JsonContent.Create(new
        {
            status = (int)AiReviewStatus.Modified,
            overrideReason = "Doctor adjusted clinical impression based on CBC."
        });

        var response = await _client.SendAsync(req);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
