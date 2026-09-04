using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class MedicalImage : TenantEntity
{
    public Guid RadiologyStudyId { get; set; }
    public RadiologyStudy RadiologyStudy { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<MedicalAnnotation> Annotations { get; set; } = [];
}
