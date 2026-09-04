using Microsoft.AspNetCore.Http;

namespace MedClinic.Application.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
}
