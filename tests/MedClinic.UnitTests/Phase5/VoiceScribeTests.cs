using FluentAssertions;
using MedClinic.Application.Features.VoiceScribe.Commands;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using Moq;
using Xunit;

namespace MedClinic.UnitTests.Phase5;

public class VoiceScribeTests
{
    [Fact]
    public async Task TranscribeVoiceNote_WhenAlreadyTranscribed_ShouldReturnFailure()
    {
        // Arrange
        var context = MockDbContext.CreateWithVoiceNote(status: VoiceNoteStatus.Completed);
        var currentUser = MockCurrentUser.Create();
        var aiProvider = new Mock<IAIProvider>();
        var handler = new TranscribeVoiceNoteCommandHandler(context, currentUser, aiProvider.Object);

        // Act
        var result = await handler.Handle(new TranscribeVoiceNoteCommand(context.VoiceNoteId), default);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Already transcribed"));
    }

    [Fact]
    public async Task TranscribeVoiceNote_WhenPending_ShouldCallAIAndReturnStructuredNote()
    {
        // Arrange
        var context = MockDbContext.CreateWithVoiceNote(status: VoiceNoteStatus.Pending);
        var currentUser = MockCurrentUser.Create();
        var aiProvider = new Mock<IAIProvider>();

        aiProvider.Setup(a => a.TranscribeVoiceNoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VoiceTranscriptionResult("Patient has chest pain.", "en", 0.95m));

        aiProvider.Setup(a => a.ParseClinicalNoteFromTranscriptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StructuredClinicalNote("Chest pain", "2-day history", "Normal exam", "Possible GERD", "PPI trial"));

        var handler = new TranscribeVoiceNoteCommandHandler(context, currentUser, aiProvider.Object);

        // Act
        var result = await handler.Handle(new TranscribeVoiceNoteCommand(context.VoiceNoteId), default);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.ChiefComplaint.Should().Be("Chest pain");
        result.Data.Assessment.Should().Be("Possible GERD");
    }

    [Fact]
    public void VoiceNote_DefaultStatus_ShouldBePending()
    {
        var note = new VoiceNote();
        note.Status.Should().Be(VoiceNoteStatus.Pending);
        note.DoctorApproved.Should().BeFalse();
    }

    [Fact]
    public void VoiceNote_OnApproval_ShouldSetApprovedFlag()
    {
        var note = new VoiceNote
        {
            DoctorApproved = true,
            DoctorApprovedAt = DateTime.UtcNow
        };
        note.DoctorApproved.Should().BeTrue();
        note.DoctorApprovedAt.Should().NotBeNull();
    }
}
