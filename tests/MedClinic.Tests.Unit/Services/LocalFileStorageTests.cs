using FluentAssertions;
using MedClinic.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class LocalFileStorageTests
{
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:LocalPath"] = Path.Combine(Path.GetTempPath(), "medclinic_tests_" + Guid.NewGuid().ToString("N")),
                ["Storage:BaseUrl"] = "http://localhost:5000/uploads"
            })
            .Build();

        _storage = new LocalFileStorage(config);
    }

    [Fact]
    public async Task UploadAsync_DisallowedExtension_ThrowsArgumentException()
    {
        using var stream = new MemoryStream([0x4D, 0x5A, 0x90, 0x00]); // MZ executable header
        var act = async () => await _storage.UploadAsync(stream, "malware.exe", "application/octet-stream", "test");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not permitted*");
    }

    [Fact]
    public async Task UploadAsync_MismatchedMagicBytes_ThrowsInvalidOperationException()
    {
        // Declared as png, but stream contains plain text
        using var stream = new MemoryStream("NOT_A_PNG_FILE"u8.ToArray());
        var act = async () => await _storage.UploadAsync(stream, "image.png", "image/png", "test");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*does not match PNG format*");
    }

    [Fact]
    public async Task UploadAsync_ValidPngMagicBytes_Succeeds()
    {
        // Valid PNG header: 89 50 4E 47 0D 0A 1A 0A
        byte[] validPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
        using var stream = new MemoryStream(validPng);

        var url = await _storage.UploadAsync(stream, "valid.png", "image/png", "test");
        url.Should().NotBeNullOrWhiteSpace();
        url.Should().EndWith(".png");
    }
}
