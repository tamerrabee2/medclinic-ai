using MedClinic.Application.Interfaces;
using MedClinic.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MedClinic.Infrastructure.Storage;

/// <summary>
/// Local disk storage for development.
/// Replace with S3 / Azure Blob in production.
/// </summary>
public class LocalFileStorage : IFileStorage, IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl  = configuration["Storage:BaseUrl"]   ?? "http://localhost:5000/uploads";
        Directory.CreateDirectory(_basePath);
    }

    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
        ".pdf", ".csv", ".txt",
        ".dcm",
        ".wav", ".mp3", ".m4a", ".webm"
    };

    private static readonly HashSet<string> _disallowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".vbs", ".js", ".jsp", ".php", ".asp", ".aspx", ".cgi", ".msi", ".com", ".scr"
    };

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        ValidateExtension(ext);
        await ValidateMagicBytesAsync(stream, ext);

        // Security: Purely generated unique filename (never use client-supplied filename on disk)
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(folder, uniqueName);
        return await UploadAsync(path, stream, contentType, cancellationToken);
    }

    public async Task<string> UploadAsync(string path, Stream stream, string contentType, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(path);
        ValidateExtension(ext);

        var fullPath = Path.Combine(_basePath, path.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await stream.CopyToAsync(fileStream, ct);

        return $"{_baseUrl}/{path.TrimStart('/')}";
    }

    public Task<Stream> DownloadAsync(string fileUrl, CancellationToken ct = default)
    {
        var relative = fileUrl.Replace(_baseUrl, string.Empty).TrimStart('/');
        var fullPath = Path.Combine(_basePath, relative.Replace('/', Path.DirectorySeparatorChar));
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName);
        ValidateExtension(ext);

        await using var openStream = file.OpenReadStream();
        await ValidateMagicBytesAsync(openStream, ext);

        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);

        // Pure generated GUID name
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var fullPath   = Path.Combine(dir, uniqueName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return $"{_baseUrl}/{folder}/{uniqueName}";
    }

    private static void ValidateExtension(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext) || !_allowedExtensions.Contains(ext) || _disallowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"Files with extension '{ext}' are not permitted. Only approved clinical formats are allowed.");
        }
    }

    private static async Task ValidateMagicBytesAsync(Stream stream, string ext)
    {
        if (!stream.CanSeek || stream.Length < 4) return;

        var initialPos = stream.Position;
        try
        {
            var header = new byte[8];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length));
            if (bytesRead < 4) return;

            // Normalize
            var isJpeg = ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
            var isPng = ext.Equals(".png", StringComparison.OrdinalIgnoreCase);
            var isPdf = ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
            var isWav = ext.Equals(".wav", StringComparison.OrdinalIgnoreCase);

            if (isJpeg && (header[0] != 0xFF || header[1] != 0xD8 || header[2] != 0xFF))
            {
                throw new InvalidOperationException("File content signature does not match JPEG format.");
            }
            if (isPng && (header[0] != 0x89 || header[1] != 0x50 || header[2] != 0x4E || header[3] != 0x47))
            {
                throw new InvalidOperationException("File content signature does not match PNG format.");
            }
            if (isPdf && (header[0] != 0x25 || header[1] != 0x50 || header[2] != 0x44 || header[3] != 0x46)) // %PDF
            {
                throw new InvalidOperationException("File content signature does not match PDF format.");
            }
            if (isWav && (header[0] != 0x52 || header[1] != 0x49 || header[2] != 0x46 || header[3] != 0x46)) // RIFF
            {
                throw new InvalidOperationException("File content signature does not match WAV audio format.");
            }
        }
        finally
        {
            stream.Seek(initialPos, SeekOrigin.Begin);
        }
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
