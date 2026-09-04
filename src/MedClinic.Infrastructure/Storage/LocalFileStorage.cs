using MedClinic.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MedClinic.Infrastructure.Storage;

/// <summary>
/// Local disk storage for development.
/// Replace with S3 / Azure Blob in production (Phase 6).
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl  = configuration["Storage:BaseUrl"]   ?? "http://localhost:5000/uploads";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);

        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath   = Path.Combine(dir, uniqueName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return $"{_baseUrl}/{folder}/{uniqueName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var relative = fileUrl.Replace(_baseUrl, string.Empty).TrimStart('/');
        var fullPath = Path.Combine(_basePath, relative.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string fileUrl, CancellationToken ct = default)
    {
        var relative = fileUrl.Replace(_baseUrl, string.Empty).TrimStart('/');
        var fullPath = Path.Combine(_basePath, relative.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(fullPath));
    }
}
