namespace MedClinic.Application.Common.Interfaces;

/// <summary>
/// Abstraction over any PACS/DICOMweb provider (Orthanc, DCM4CHEE, cloud PACS, etc.).
/// </summary>
public interface IPacsProvider
{
    /// <summary>Upload a DICOM file and return the Study/Series/Instance UIDs.</summary>
    Task<DicomUploadResult> UploadDicomFileAsync(Stream dicomStream, string filename, CancellationToken ct = default);

    /// <summary>Query studies for a patient using QIDO-RS.</summary>
    Task<List<DicomStudyMetadata>> QueryStudiesAsync(string patientId, CancellationToken ct = default);

    /// <summary>Retrieve WADO-RS URL for a specific instance frame.</summary>
    string GetWadoRsUrl(string studyUid, string seriesUid, string instanceUid, int frame = 1);

    /// <summary>Retrieve the OHIF viewer URL for a study (if configured).</summary>
    string? GetViewerUrl(string studyUid);

    /// <summary>Delete a study from PACS (admin only).</summary>
    Task<bool> DeleteStudyAsync(string studyUid, CancellationToken ct = default);
}

public record DicomUploadResult(
    bool Success,
    string? StudyInstanceUid,
    string? SeriesInstanceUid,
    string? SopInstanceUid,
    string? ErrorMessage
);

public record DicomStudyMetadata(
    string StudyInstanceUid,
    string? AccessionNumber,
    string? StudyDescription,
    DateTime? StudyDate,
    string Modality,
    int SeriesCount,
    int InstanceCount
);
