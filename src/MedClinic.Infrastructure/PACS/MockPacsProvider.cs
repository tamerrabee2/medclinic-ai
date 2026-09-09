using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.PACS;

/// <summary>
/// Mock PACS provider for development and testing (zero external dependencies).
/// </summary>
public class MockPacsProvider : IPacsProvider
{
    private readonly ILogger<MockPacsProvider> _logger;

    public MockPacsProvider(ILogger<MockPacsProvider> logger)
        => _logger = logger;

    public Task<DicomUploadResult> UploadDicomFileAsync(Stream dicomStream, string filename, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockPACS] Upload: {Filename}", filename);
        return Task.FromResult(new DicomUploadResult(
            Success: true,
            StudyInstanceUid: $"1.2.840.10008.5.1.4.1.1.{Guid.NewGuid():N}",
            SeriesInstanceUid: $"1.2.840.10008.5.1.4.1.2.{Guid.NewGuid():N}",
            SopInstanceUid: $"1.2.840.10008.5.1.4.1.3.{Guid.NewGuid():N}",
            ErrorMessage: null
        ));
    }

    public Task<List<DicomStudyMetadata>> QueryStudiesAsync(string patientId, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockPACS] Query studies for patient {PatientId}", patientId);
        return Task.FromResult(new List<DicomStudyMetadata>
        {
            new(
                StudyInstanceUid: "1.2.840.10008.5.1.4.1.1.mock001",
                AccessionNumber: "ACC-2026-001",
                StudyDescription: "Chest CT — PA & Lateral",
                StudyDate: DateTime.UtcNow.AddDays(-7),
                Modality: "CT",
                SeriesCount: 3,
                InstanceCount: 120
            ),
            new(
                StudyInstanceUid: "1.2.840.10008.5.1.4.1.1.mock002",
                AccessionNumber: "ACC-2026-002",
                StudyDescription: "Right Knee X-Ray",
                StudyDate: DateTime.UtcNow.AddDays(-14),
                Modality: "CR",
                SeriesCount: 1,
                InstanceCount: 2
            )
        });
    }

    public string GetWadoRsUrl(string studyUid, string seriesUid, string instanceUid, int frame = 1)
        => $"/mock-pacs/wado-rs/studies/{studyUid}/series/{seriesUid}/instances/{instanceUid}/frames/{frame}";

    public string? GetViewerUrl(string studyUid)
        => $"/mock-pacs/viewer?StudyInstanceUIDs={studyUid}";

    public Task<bool> DeleteStudyAsync(string studyUid, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockPACS] Delete study {StudyUid}", studyUid);
        return Task.FromResult(true);
    }
}
