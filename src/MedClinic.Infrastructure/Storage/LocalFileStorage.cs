using MedClinic.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MedClinic.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf", ".dcm"
    };

    private static readonly HashSet<string> _allowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf", "application/dicom"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    public LocalFileStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = configuration["Storage:BaseUrl"] ?? "/files";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(fileName, contentType, fileStream.Length);

        var safeFolder = SanitizePath(folder);
        var uploadDir = Path.Combine(_basePath, safeFolder);
        Directory.CreateDirectory(uploadDir);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName).ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDir, uniqueFileName);

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fs, cancellationToken);

        return $"{_baseUrl}/{safeFolder}/{uniqueFileName}";
    }

    public async Task<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePhysicalPath(filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", filePath);

        var memoryStream = new MemoryStream();
        await using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        await fs.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePhysicalPath(filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePhysicalPath(filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private void ValidateFile(string fileName, string contentType, long fileSize)
    {
        if (fileSize > MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(fileName);
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File extension '{extension}' is not allowed.");

        if (!_allowedMimeTypes.Contains(contentType))
            throw new InvalidOperationException($"Content type '{contentType}' is not allowed.");
    }

    private string ResolvePhysicalPath(string fileUrl)
    {
        var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
        return Path.Combine(_basePath, SanitizePath(relativePath));
    }

    private static string SanitizePath(string path)
    {
        return path.Replace("..", "").Trim('/', '\\').Replace('\\', '/');
    }
}
