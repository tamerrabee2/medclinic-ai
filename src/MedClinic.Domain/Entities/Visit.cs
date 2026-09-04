using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Visit : TenantEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;
    public string? ChiefComplaint { get; set; }
    public string? Symptoms { get; set; }
    public Vitals? Vitals { get; set; }
    public string? PhysicalExamination { get; set; }
    public string? Diagnosis { get; set; }
    public string? DifferentialDiagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? DoctorNotes { get; set; }
    public string? FollowUpNotes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.InProgress;

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<LabOrder> LabOrders { get; set; } = [];
}

public class Vitals
{
    public decimal? Temperature { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? BMI { get; set; }
}

public enum VisitStatus { InProgress, Completed, Cancelled }
