using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Clinic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? LicenseNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public ClinicPlan Plan { get; set; } = ClinicPlan.Free;
    public DateTime? PlanExpiresAt { get; set; }

    public ICollection<ClinicMember> Members { get; set; } = [];
    public ICollection<Doctor> Doctors { get; set; } = [];
    public ICollection<Patient> Patients { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}

public enum ClinicPlan { Free, Basic, Professional, Enterprise }
