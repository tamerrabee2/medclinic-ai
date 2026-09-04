namespace MedClinic.Domain.Entities;

public class DentalRecord : BaseEntity
{
    public Guid   PatientId { get; set; }
    public Guid   ClinicId  { get; set; }
    public Guid?  VisitId   { get; set; }
    public Guid   DoctorId  { get; set; }

    /// <summary>Tooth number using FDI notation (11-18, 21-28, 31-38, 41-48)</summary>
    public int    ToothNumber { get; set; }

    public string Condition   { get; set; } = string.Empty; // Cavity, Filling, Crown, Missing, Implant, RootCanal, Extraction, Healthy
    public string? Surface    { get; set; } // Mesial, Distal, Occlusal, Buccal, Lingual
    public string? Notes      { get; set; }
    public DateTime? TreatmentDate { get; set; }

    // Navigation
    public Patient? Patient { get; set; }
    public Doctor?  Doctor  { get; set; }
    public Visit?   Visit   { get; set; }
}
