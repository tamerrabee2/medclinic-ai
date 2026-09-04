using Microsoft.AspNetCore.Http;

namespace MedClinic.Application.Interfaces;

public interface IFileStorage
{
    /// <summary>Save file and return public URL</summary>
    Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default);

    /// <summary>Delete file by its URL or path</summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>Check if a file exists</summary>
    Task<bool> ExistsAsync(string fileUrl, CancellationToken ct = default);
}
