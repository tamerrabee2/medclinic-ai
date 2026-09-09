using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

/// <summary>
/// Represents a DICOM study (e.g., one CT scan session).
/// </summary>
public class DicomStudy : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? VisitId { get; set; }

    // DICOM identifiers
    public string StudyInstanceUid { get; set; } = string.Empty;
    public string? AccessionNumber { get; set; }
    public string? StudyDescription { get; set; }
    public DateTime StudyDate { get; set; }
    public string Modality { get; set; } = string.Empty; // CT, MR, CR, US, DX, etc.

    // PACS location
    public string? PacsStudyUrl { get; set; }
    public string? WadoRsBaseUrl { get; set; }
    public DicomStudyStatus Status { get; set; } = DicomStudyStatus.Pending;

    // Counts
    public int SeriesCount { get; set; }
    public int InstanceCount { get; set; }

    // AI analysis (requires physician review)
    public bool HasAIAnalysis { get; set; } = false;
    public string? AIFindings { get; set; }
    public bool AIReviewedByDoctor { get; set; } = false;

    // Navigation
    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ICollection<DicomSeries> Series { get; set; } = new List<DicomSeries>();
}

public class DicomSeries : BaseAuditableEntity
{
    public Guid DicomStudyId { get; set; }
    public string SeriesInstanceUid { get; set; } = string.Empty;
    public int SeriesNumber { get; set; }
    public string? SeriesDescription { get; set; }
    public string Modality { get; set; } = string.Empty;
    public int InstanceCount { get; set; }
    public string? BodyPartExamined { get; set; }

    public DicomStudy DicomStudy { get; set; } = null!;
    public ICollection<DicomInstance> Instances { get; set; } = new List<DicomInstance>();
}

public class DicomInstance : BaseAuditableEntity
{
    public Guid DicomSeriesId { get; set; }
    public string SopInstanceUid { get; set; } = string.Empty;
    public string SopClassUid { get; set; } = string.Empty;
    public int InstanceNumber { get; set; }
    public string? WadoUri { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? Rows { get; set; }
    public int? Columns { get; set; }

    public DicomSeries DicomSeries { get; set; } = null!;
}

public enum DicomStudyStatus
{
    Pending = 0,
    Uploading = 1,
    Available = 2,
    Processing = 3,
    Failed = 4
}
