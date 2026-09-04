using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class Appointment : TenantEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool IsFollowUp { get; set; } = false;
    public Guid? PreviousAppointmentId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public Visit? Visit { get; set; }
}
