using Microsoft.AspNetCore.Http;

namespace MedClinic.Application.Interfaces;

public interface IFileStorage
{
    /// <summary>Upload a stream to storage and return URL or path</summary>
    Task<string> UploadAsync(string path, Stream stream, string contentType, CancellationToken ct = default);

    /// <summary>Download a stream by path or URL</summary>
    Task<Stream> DownloadAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>Save form file and return public URL</summary>
    Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default);

    /// <summary>Delete file by its URL or path</summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>Check if a file exists</summary>
    Task<bool> ExistsAsync(string fileUrl, CancellationToken ct = default);
}
