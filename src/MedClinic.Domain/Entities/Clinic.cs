using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class Clinic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Speciality { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Timezone { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? Language { get; set; } = "en";

    public ICollection<ClinicMember> Members { get; set; } = [];
    public ICollection<Patient> Patients { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
}
