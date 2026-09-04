using MedClinic.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MedClinic.Infrastructure.Storage;

public class LocalFileStorage(IConfiguration configuration) : IFileStorage
{
    private readonly string _basePath = configuration["STORAGE_LOCAL_PATH"] ?? "./storage";

    public async Task<string> UploadAsync(
        Stream fileStream, string fileName, string contentType,
        string folder, CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);
        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(folderPath, uniqueName);
        await using var fs = File.Create(filePath);
        await fileStream.CopyToAsync(fs, cancellationToken);
        return Path.Combine(folder, uniqueName).Replace("\\", "/");
    }

    public async Task<Stream> DownloadAsync(
        string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found", filePath);
        return await Task.FromResult(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<string> GetSignedUrlAsync(
        string filePath, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        // For local storage, return a protected API endpoint URL
        return Task.FromResult($"/api/v1/files/download?path={Uri.EscapeDataString(filePath)}");
    }
}
