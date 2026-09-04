using MedClinic.Domain.Common;
using MedClinic.Domain.Enums;

namespace MedClinic.Domain.Entities;

public class Patient : TenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; } = BloodType.Unknown;
    public string? NationalId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AvatarUrl { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<LabOrder> LabOrders { get; set; } = [];
    public ICollection<RadiologyStudy> RadiologyStudies { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<AIAnalysis> AIAnalyses { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
