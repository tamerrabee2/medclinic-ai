using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class MedicalAnnotation : BaseEntity
{
    public Guid MedicalImageId { get; set; }
    public Guid DoctorId { get; set; }
    public string Type { get; set; } = string.Empty; // Pen, Arrow, Rectangle, Circle, Text, Measurement
    public string? CoordinatesJson { get; set; } // JSON array of coordinates
    public string? Color { get; set; }
    public double? Thickness { get; set; }
    public string? Text { get; set; }
    public int ZIndex { get; set; } = 0;

    public MedicalImage MedicalImage { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}
