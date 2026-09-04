using FluentAssertions;
using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Features.AI.Services;
using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure.AI;
using MedClinic.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class AIServiceTests
{
    private readonly FakeTenantContext _tenant;
    private readonly IAIProvider       _mockProvider;

    public AIServiceTests()
    {
        _tenant       = new FakeTenantContext();
        _mockProvider = new MockAIProvider(NullLogger<MockAIProvider>.Instance);
    }

    [Fact]
    public async Task SendMessageAsync_CreatesNewConversation_WhenNoConversationId()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var userId  = _tenant.UserId;
        var service = new AIService(db, _tenant, _mockProvider,
            NullLogger<AIService>.Instance);

        var req = new SendMessageRequest(
            ConversationId:   null,
            Message:          "What are the symptoms of diabetes?",
            PatientContextId: null,
            AttachmentBase64: null,
            AttachmentMimeType: null
        );

        // Act
        var result = await service.SendMessageAsync(userId, req);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Messages.Should().HaveCount(2); // user + assistant
        result.Messages[0].Role.Should().Be("user");
        result.Messages[1].Role.Should().Be("assistant");
    }

    [Fact]
    public async Task SendMessageAsync_ContinuesExistingConversation()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var userId  = _tenant.UserId;
        var service = new AIService(db, _tenant, _mockProvider,
            NullLogger<AIService>.Instance);

        var req1 = new SendMessageRequest(
            ConversationId: null,
            Message:        "Hello Dr. AI",
            PatientContextId: null,
            AttachmentBase64: null,
            AttachmentMimeType: null
        );

        var first = await service.SendMessageAsync(userId, req1);

        var req2 = new SendMessageRequest(
            ConversationId: first.Id,
            Message:        "What is hypertension?",
            PatientContextId: null,
            AttachmentBase64: null,
            AttachmentMimeType: null
        );

        // Act
        var result = await service.SendMessageAsync(userId, req2);

        // Assert
        result.Id.Should().Be(first.Id);
        result.Messages.Should().HaveCount(4); // 2 previous + 2 new
    }

    [Fact]
    public async Task GetConversationsAsync_ReturnsOnlyUserConversations()
    {
        // Arrange
        var db       = TestDbContextFactory.Create();
        var userA    = _tenant.UserId;
        var userB    = Guid.NewGuid();
        var service  = new AIService(db, _tenant, _mockProvider,
            NullLogger<AIService>.Instance);

        // Create conversation for user A
        await service.SendMessageAsync(userA, new(
            null, "Question from A", null, null, null));

        // Create conversation for user B directly
        db.AIConversations.Add(new()
        {
            Id        = Guid.NewGuid(),
            UserId    = userB,
            ClinicId  = _tenant.ClinicId,
            Title     = "User B conv",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages  = []
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetConversationsAsync(userA);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteConversationAsync_RemovesConversation()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var userId  = _tenant.UserId;
        var service = new AIService(db, _tenant, _mockProvider,
            NullLogger<AIService>.Instance);

        var conv = await service.SendMessageAsync(userId, new(
            null, "Test message", null, null, null));

        // Act
        await service.DeleteConversationAsync(conv.Id, userId);

        // Assert
        var conversations = await service.GetConversationsAsync(userId);
        conversations.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeLabResultAsync_ReturnsAnalysis_WithAbnormalities()
    {
        // Arrange
        var db       = TestDbContextFactory.Create();
        var labOrder = new MedClinic.Domain.Entities.LabOrder
        {
            Id        = Guid.NewGuid(),
            ClinicId  = _tenant.ClinicId,
            PatientId = Guid.NewGuid(),
            DoctorId  = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status    = "Completed"
        };
        db.LabOrders.Add(labOrder);

        var labResult = TestDataBuilder.BuildLabResult(orderId: labOrder.Id);
        labResult.Items =
        [
            new() { Id = Guid.NewGuid(), LabResultId = labResult.Id, TestName = "Hemoglobin",
                    Value = "8.0", Unit = "g/dL", ReferenceRange = "13.5-17.5",
                    AbnormalFlag = "L", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), LabResultId = labResult.Id, TestName = "WBC",
                    Value = "6.0", Unit = "10^3/uL", ReferenceRange = "4.5-11.0",
                    AbnormalFlag = "", CreatedAt = DateTime.UtcNow }
        ];
        db.LabResults.Add(labResult);
        await db.SaveChangesAsync();

        var service = new AIService(db, _tenant, _mockProvider,
            NullLogger<AIService>.Instance);

        // Act
        var result = await service.AnalyzeLabResultAsync(
            _tenant.UserId,
            new AnalyzeLabRequest(labResult.Id, true));

        // Assert
        result.Should().NotBeNull();
        result.Abnormalities.Should().HaveCount(1);
        result.Abnormalities[0].Should().Contain("Hemoglobin");
        result.RequiresDoctorReview.Should().BeTrue();
        result.Disclaimer.Should().NotBeNullOrEmpty();
    }
}
