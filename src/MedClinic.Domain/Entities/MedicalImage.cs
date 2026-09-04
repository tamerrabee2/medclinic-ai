using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class MedicalImage : TenantEntity
{
    public Guid RadiologyStudyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string? AnnotatedUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Modality { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsAIAnalyzed { get; set; } = false;

    public RadiologyStudy RadiologyStudy { get; set; } = null!;
    public ICollection<MedicalAnnotation> Annotations { get; set; } = [];
    public ICollection<AIAnalysis> AIAnalyses { get; set; } = [];
}
