using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class MedicalImage : BaseEntity
{
    public Guid    RadiologyStudyId { get; set; }
    public string  FileName         { get; set; } = string.Empty;
    public string  FileUrl          { get; set; } = string.Empty;
    public long    FileSizeBytes    { get; set; }
    public string? ContentType      { get; set; }
    public string? Modality         { get; set; }  // X-Ray, CT, MRI, Ultrasound
    public int?    SeriesNumber     { get; set; }  // DICOM series
    public int?    InstanceNumber   { get; set; }  // DICOM instance
    public DateTime UploadedAt      { get; set; } = DateTime.UtcNow;

    public RadiologyStudy RadiologyStudy { get; set; } = null!;
}
