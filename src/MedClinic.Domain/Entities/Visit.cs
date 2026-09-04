using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Visit : TenantEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;
    public string? ChiefComplaint { get; set; }
    public string? Symptoms { get; set; }
    public string? VitalsJson { get; set; }
    public string? PhysicalExamination { get; set; }
    public string? Diagnosis { get; set; }
    public string? DifferentialDiagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? FollowUpInstructions { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<LabOrder> LabOrders { get; set; } = [];
    public ICollection<RadiologyStudy> RadiologyStudies { get; set; } = [];
}
