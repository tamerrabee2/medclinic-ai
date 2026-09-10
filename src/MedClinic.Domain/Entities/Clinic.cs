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
    [System.Text.Json.Serialization.JsonIgnore]
    public string Timezone { get => TimeZone; set => TimeZone = value; }
    public string Currency { get; set; } = "USD";
    public string InvoicePrefix { get; set; } = "INV";
    public decimal TaxRate { get; set; } = 0m;
    public int DefaultAppointmentDuration { get; set; } = 30;
    public int? MaxDailyAppointments { get; set; }
    public bool AllowOnlineBooking { get; set; } = true;
    public TimeOnly? WorkingHoursStart { get; set; }
    public TimeOnly? WorkingHoursEnd { get; set; }
    public string? WorkingDays { get; set; }
    public bool IsActive { get; set; } = true;
    public ClinicPlan Plan { get; set; } = ClinicPlan.Free;
    public DateTime? PlanExpiresAt { get; set; }

    // ── SaaS Multi-Tenancy & Lifecycle Governance ──────────────────────────
    public Enums.ClinicLifecycleStatus LifecycleStatus { get; set; } = Enums.ClinicLifecycleStatus.Trial;
    public Enums.BillingStatus BillingStatus { get; set; } = Enums.BillingStatus.Current;
    public Enums.TenantComplianceStatus ComplianceStatus { get; set; } = Enums.TenantComplianceStatus.Normal;
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }
    public Guid? SuspendedByUserId { get; set; }

    public ICollection<ClinicMember> Members { get; set; } = [];
    public ICollection<Doctor> Doctors { get; set; } = [];
    public ICollection<Patient> Patients { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<ClinicSubscription> Subscriptions { get; set; } = [];
    public ICollection<UsageMetric> UsageMetrics { get; set; } = [];
    public ICollection<TenantLifecycleAuditEvent> LifecycleEvents { get; set; } = [];
}

public enum ClinicPlan { Free, Basic, Professional, Enterprise }
