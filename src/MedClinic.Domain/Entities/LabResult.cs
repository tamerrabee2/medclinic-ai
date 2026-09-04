using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class LabResult : TenantEntity
{
    public Guid LabOrderId { get; set; }
    public LabOrder LabOrder { get; set; } = null!;
    public DateTime ResultDate { get; set; } = DateTime.UtcNow;
    public string? FileUrl { get; set; }
    public string? Notes { get; set; }
    public bool IsAIAnalyzed { get; set; } = false;
    public string? AIAnalysisSummary { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<LabResultItem> Items { get; set; } = [];
}
