using FluentAssertions;
using MedClinic.Application.Features.Canvas.DTOs;
using MedClinic.Application.Features.Canvas.Services;
using MedClinic.Application.Interfaces;
using MedClinic.Tests.Unit.Helpers;
using Moq;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class CanvasServiceTests
{
    private readonly FakeTenantContext _tenant;
    private readonly Mock<IFileStorage> _storageMock;

    public CanvasServiceTests()
    {
        _tenant      = new FakeTenantContext();
        _storageMock = new Mock<IFileStorage>();
        _storageMock.Setup(s => s.UploadAsync(
            It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("mocked/path.jpg");
    }

    [Fact]
    public async Task AddAnnotationAsync_SavesAnnotation()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var imageId  = Guid.NewGuid();
        var service  = new CanvasService(db, _tenant, _storageMock.Object);

        var req = new CreateAnnotationRequest(
            MedicalImageId: imageId,
            Type:           "Rectangle",
            CoordinatesJson: "[{\"x\":10,\"y\":20},{\"x\":100,\"y\":120}]",
            Color:           "#FF0000",
            Thickness:       2,
            Text:            null,
            MeasurementValue: null,
            MeasurementUnit:  null
        );

        // Act
        var result = await service.AddAnnotationAsync(doctorId, req);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("Rectangle");
        result.MedicalImageId.Should().Be(imageId);

        var saved = db.MedicalAnnotations.First();
        saved.DoctorId.Should().Be(doctorId);
        saved.Color.Should().Be("#FF0000");
    }

    [Fact]
    public async Task GetAnnotationsAsync_ReturnsAnnotationsForImage()
    {
        // Arrange
        var db       = TestDbContextFactory.Create();
        var imageId  = Guid.NewGuid();
        var doctorId = Guid.NewGuid();

        db.MedicalAnnotations.AddRange(
            new() { Id = Guid.NewGuid(), MedicalImageId = imageId,  DoctorId = doctorId,
                    Type = "Arrow", CoordinatesJson = "[]", Color = "#00FF00",
                    Thickness = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), MedicalImageId = imageId,  DoctorId = doctorId,
                    Type = "Circle", CoordinatesJson = "[]", Color = "#0000FF",
                    Thickness = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), MedicalImageId = Guid.NewGuid(), DoctorId = doctorId,
                    Type = "Pen", CoordinatesJson = "[]", Color = "#FF0000",
                    Thickness = 2, CreatedAt = DateTime.UtcNow } // different image
        );
        await db.SaveChangesAsync();

        var service = new CanvasService(db, _tenant, _storageMock.Object);

        // Act
        var result = await service.GetAnnotationsAsync(imageId);

        // Assert
        result.Should().HaveCount(2);
        result.All(a => a.MedicalImageId == imageId).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAnnotationAsync_RemovesAnnotation_WhenOwner()
    {
        // Arrange
        var db       = TestDbContextFactory.Create();
        var doctorId = Guid.NewGuid();
        var annotId  = Guid.NewGuid();

        db.MedicalAnnotations.Add(new()
        {
            Id = annotId, MedicalImageId = Guid.NewGuid(), DoctorId = doctorId,
            Type = "Text", CoordinatesJson = "[]", Thickness = 1,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new CanvasService(db, _tenant, _storageMock.Object);

        // Act
        await service.DeleteAnnotationAsync(annotId, doctorId);

        // Assert
        db.MedicalAnnotations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAnnotationAsync_ThrowsKeyNotFound_WhenNotOwner()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var annotId = Guid.NewGuid();

        db.MedicalAnnotations.Add(new()
        {
            Id = annotId, MedicalImageId = Guid.NewGuid(), DoctorId = ownerId,
            Type = "Pen", CoordinatesJson = "[]", Thickness = 1,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new CanvasService(db, _tenant, _storageMock.Object);

        // Act
        var act = async () => await service.DeleteAnnotationAsync(annotId, otherId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
