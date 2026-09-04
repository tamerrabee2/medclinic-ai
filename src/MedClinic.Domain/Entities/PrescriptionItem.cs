using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public string? Route { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public int? Quantity { get; set; }
    public int? Refills { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
