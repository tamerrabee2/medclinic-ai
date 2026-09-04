namespace MedClinic.Application.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task<string> GetSignedUrlAsync(
        string filePath,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}
