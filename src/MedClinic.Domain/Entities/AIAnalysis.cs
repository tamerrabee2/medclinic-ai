using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIAnalysis : BaseEntity
{
    public Guid    RadiologyStudyId { get; set; }
    public string  AnalysisType     { get; set; } = string.Empty; // e.g. "ChestXRay", "BoneAge"
    public string? Findings         { get; set; }  // AI-generated findings text
    public decimal? Confidence      { get; set; }  // 0.0 – 1.0
    public string? ModelVersion     { get; set; }  // e.g. "medclinic-vision-v1.2"
    public string? RawResponse      { get; set; }  // JSON from AI provider
    public DateTime AnalyzedAt      { get; set; } = DateTime.UtcNow;
    public bool   IsReviewed        { get; set; } = false;
    public string? ReviewedBy       { get; set; }
    public DateTime? ReviewedAt     { get; set; }

    public RadiologyStudy RadiologyStudy { get; set; } = null!;
}
