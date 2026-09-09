using FluentAssertions;
using MedClinic.Application.Features.VoiceScribe.Commands;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        var handler = new TranscribeVoiceNoteCommandHandler(context.Db, currentUser, aiProvider.Object);

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

        var handler = new TranscribeVoiceNoteCommandHandler(context.Db, currentUser, aiProvider.Object);

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

public static class MockDbContext
{
    public class TestContext
    {
        public ApplicationDbContext Db { get; set; } = null!;
        public Guid VoiceNoteId { get; set; }
    }

    public static TestContext CreateWithVoiceNote(VoiceNoteStatus status)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        var clinicId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var note = new VoiceNote
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            AudioFileUrl = "https://storage/audio.mp3",
            Status = status
        };
        db.VoiceNotes.Add(note);
        db.SaveChanges();
        return new TestContext { Db = db, VoiceNoteId = note.Id };
    }
}

public static class MockCurrentUser
{
    public static ICurrentUserService Create()
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(m => m.ClinicId).Returns(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        mock.Setup(m => m.UserId).Returns(Guid.NewGuid());
        return mock.Object;
    }
}
