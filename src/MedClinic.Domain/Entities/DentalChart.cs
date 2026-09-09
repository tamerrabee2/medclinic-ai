using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class DentalChart : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ICollection<ToothRecord> Teeth { get; set; } = new List<ToothRecord>();
}

public class ToothRecord : BaseAuditableEntity
{
    public Guid DentalChartId { get; set; }
    public int ToothNumber { get; set; }
    public ToothStatus Status { get; set; } = ToothStatus.Normal;
    public string? Notes { get; set; }
    public string SurfaceDataJson { get; set; } = "{}";

    public DentalChart DentalChart { get; set; } = null!;
}

public enum ToothStatus
{
    Normal = 0,
    Caries = 1,
    Filling = 2,
    Crown = 3,
    Missing = 4,
    Implant = 5,
    RootCanal = 6,
    Extraction = 7
}
