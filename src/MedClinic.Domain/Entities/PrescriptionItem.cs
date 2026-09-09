using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public Guid   PrescriptionId { get; set; }
    public string MedicineName   { get; set; } = string.Empty;
    public string MedicationName { get => MedicineName; set => MedicineName = value; }
    public string? Dosage        { get; set; }  // e.g. "500mg"
    public string? Dose          { get => Dosage; set => Dosage = value; }
    public string? Frequency     { get; set; }  // e.g. "3 times daily"
    public int?    DurationDays  { get; set; }
    public string? Duration      { get => DurationDays?.ToString(); set { if (int.TryParse(value, out var d)) DurationDays = d; } }
    public string? Route         { get; set; }  // Oral, IV, Topical ...
    public string? Instructions  { get; set; }  // "Take after meals"
    public int?    Quantity      { get; set; }  // Total units to dispense
    public string? Notes         { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
