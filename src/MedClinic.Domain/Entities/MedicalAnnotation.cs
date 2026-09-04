namespace MedClinic.Domain.Entities;

public class MedicalAnnotation : BaseEntity
{
    public Guid        MedicalImageId { get; set; }
    public Guid        DoctorId       { get; set; }
    public string      Type           { get; set; } = string.Empty; // Pen, Arrow, Rectangle, Circle, Text, Measurement
    public string      CoordinatesJson { get; set; } = "[]";         // JSON array of points
    public string?     Color          { get; set; } = "#FF0000";
    public int         Thickness      { get; set; } = 2;
    public string?     Text           { get; set; }
    public double?     MeasurementValue { get; set; }
    public string?     MeasurementUnit  { get; set; }               // mm, cm, px
    public bool        IsAIGenerated  { get; set; } = false;
    public double?     AIConfidence   { get; set; }

    // Navigation
    public RadiologyImage? Image  { get; set; }
    public User?           Doctor { get; set; }
}
