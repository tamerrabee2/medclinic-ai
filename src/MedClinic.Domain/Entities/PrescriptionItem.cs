using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public Guid   PrescriptionId { get; set; }
    public string MedicineName   { get; set; } = string.Empty;
    public string? Dosage        { get; set; }  // e.g. "500mg"
    public string? Frequency     { get; set; }  // e.g. "3 times daily"
    public int?    DurationDays  { get; set; }
    public string? Route         { get; set; }  // Oral, IV, Topical ...
    public string? Instructions  { get; set; }  // "Take after meals"
    public int?    Quantity      { get; set; }  // Total units to dispense

    public Prescription Prescription { get; set; } = null!;
}
