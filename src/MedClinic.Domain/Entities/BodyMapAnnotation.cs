namespace MedClinic.Domain.Entities;

public class BodyMapAnnotation : BaseEntity
{
    public Guid    VisitId    { get; set; }
    public Guid    DoctorId   { get; set; }
    public Guid    PatientId  { get; set; }
    public Guid    ClinicId   { get; set; }

    // Body region (Head, Neck, Chest, Abdomen, Back, LeftArm, RightArm, LeftLeg, RightLeg)
    public string  Region     { get; set; } = string.Empty;
    public string? Side       { get; set; } // Anterior / Posterior

    // Clinical data
    public string? Symptom    { get; set; }
    public int?    PainLevel  { get; set; } // 1-10
    public string? Notes      get; set; }
    public string? Diagnosis  { get; set; }

    // SVG position
    public double  PositionX  { get; set; }
    public double  PositionY  { get; set; }
    public string  MarkerColor { get; set; } = "#EF4444";

    // Navigation
    public Visit?   Visit   { get; set; }
    public Patient? Patient { get; set; }
}
