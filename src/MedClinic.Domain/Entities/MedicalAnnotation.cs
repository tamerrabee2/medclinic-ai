using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class MedicalAnnotation : TenantEntity
{
    public Guid MedicalImageId { get; set; }
    public MedicalImage MedicalImage { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string CoordinatesJson { get; set; } = string.Empty;
    public string? Color { get; set; }
    public double? Thickness { get; set; }
    public string? Text { get; set; }
    public Clinic Clinic { get; set; } = null!;
}
