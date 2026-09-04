using MedClinic.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorage> _logger;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf", ".dcm"
    };

    public LocalFileStorage(IConfiguration configuration, ILogger<LocalFileStorage> logger)
    {
        _basePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File extension '{extension}' is not allowed.");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        _logger.LogInformation("File uploaded: {Folder}/{FileName}", folder, fileName);
        return $"{folder}/{fileName}";
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        // Prevent path traversal
        var resolvedPath = Path.GetFullPath(fullPath);
        if (!resolvedPath.StartsWith(Path.GetFullPath(_basePath)))
            throw new UnauthorizedAccessException("Invalid file path.");

        return await Task.FromResult<Stream>(new FileStream(resolvedPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(fullPath));
    }
}
