using FluentAssertions;
using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Services;
using MedClinic.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class AiDecisionAuditServiceTests
{
    private readonly FakeTenantContext _tenant;

    public AiDecisionAuditServiceTests()
    {
        _tenant = new FakeTenantContext();
    }

    [Fact]
    public async Task RecordDecisionAsync_ComputesSha256Hashes_AndSetsPendingReview()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var service = new AiDecisionAuditService(db, _tenant, NullLogger<AiDecisionAuditService>.Instance);

        var request = new RecordAiDecisionRequest
        {
            Capability = "LabAnalysis",
            ProviderName = "Mock",
            ModelVersion = "gpt-4o-mini",
            InputPayload = "{\"bloodSugar\": 180, \"hba1c\": 8.5}",
            OutputPayload = "{\"diagnosis\": \"Uncontrolled Type 2 Diabetes\"}",
            ConfidenceScore = 0.94,
            CorrelationId = "corr-12345"
        };

        // Act
        var result = await service.RecordDecisionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ClinicId.Should().Be(_tenant.ClinicId);
        result.Capability.Should().Be("LabAnalysis");
        result.ReviewStatus.Should().Be(AiReviewStatus.PendingReview);
        result.InputHash.Should().HaveLength(64);
        result.OutputHash.Should().HaveLength(64);
        result.ConfidenceScore.Should().Be(0.94);
        result.CorrelationId.Should().Be("corr-12345");

        var stored = await db.AiDecisionAudits.FindAsync(result.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task ReviewDecisionAsync_Accepted_UpdatesStatusAndReviewer()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var service = new AiDecisionAuditService(db, _tenant, NullLogger<AiDecisionAuditService>.Instance);

        var audit = new AiDecisionAudit
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            Capability = "ClinicalChat",
            ProviderName = "Mock",
            ReviewStatus = AiReviewStatus.PendingReview,
            CreatedAt = DateTime.UtcNow
        };
        db.AiDecisionAudits.Add(audit);
        await db.SaveChangesAsync();

        var doctorId = Guid.NewGuid();
        var reviewReq = new ReviewAiDecisionRequest
        {
            Status = AiReviewStatus.Accepted
        };

        // Act
        var updated = await service.ReviewDecisionAsync(audit.Id, reviewReq, doctorId);

        // Assert
        updated.ReviewStatus.Should().Be(AiReviewStatus.Accepted);
        updated.ReviewedByUserId.Should().Be(doctorId);
        updated.ReviewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ReviewDecisionAsync_Rejected_WithoutOverrideReason_ThrowsArgumentException()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var service = new AiDecisionAuditService(db, _tenant, NullLogger<AiDecisionAuditService>.Instance);

        var audit = new AiDecisionAudit
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            Capability = "PrescriptionCheck",
            ProviderName = "Mock",
            ReviewStatus = AiReviewStatus.PendingReview,
            CreatedAt = DateTime.UtcNow
        };
        db.AiDecisionAudits.Add(audit);
        await db.SaveChangesAsync();

        var doctorId = Guid.NewGuid();
        var reviewReq = new ReviewAiDecisionRequest
        {
            Status = AiReviewStatus.Rejected,
            OverrideReason = null // Missing required reason
        };

        // Act
        var act = async () => await service.ReviewDecisionAsync(audit.Id, reviewReq, doctorId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*override reason*");
    }

    [Fact]
    public async Task ReviewDecisionAsync_Modified_WithOverrideReason_Succeeds()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var service = new AiDecisionAuditService(db, _tenant, NullLogger<AiDecisionAuditService>.Instance);

        var audit = new AiDecisionAudit
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            Capability = "ImageAnalysis",
            ProviderName = "Mock",
            ReviewStatus = AiReviewStatus.PendingReview,
            CreatedAt = DateTime.UtcNow
        };
        db.AiDecisionAudits.Add(audit);
        await db.SaveChangesAsync();

        var doctorId = Guid.NewGuid();
        var reviewReq = new ReviewAiDecisionRequest
        {
            Status = AiReviewStatus.Modified,
            OverrideReason = "Adjusted nodule boundaries based on lateral projection."
        };

        // Act
        var updated = await service.ReviewDecisionAsync(audit.Id, reviewReq, doctorId);

        // Assert
        updated.ReviewStatus.Should().Be(AiReviewStatus.Modified);
        updated.OverrideReason.Should().Be("Adjusted nodule boundaries based on lateral projection.");
        updated.ReviewedByUserId.Should().Be(doctorId);
    }
}
