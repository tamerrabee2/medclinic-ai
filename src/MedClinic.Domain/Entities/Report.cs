using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class ClinicReport : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public ReportType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? FileUrl { get; set; }
    public string? DataJson { get; set; }
    public Guid RequestedBy { get; set; }

    public Clinic Clinic { get; set; } = null!;
}

public enum ReportType
{
    DailyAppointments = 0,
    MonthlyRevenue = 1,
    PatientStatistics = 2,
    DoctorPerformance = 3,
    LabSummary = 4,
    ImagingSummary = 5,
    AIUsage = 6,
    FollowUpCompliance = 7
}

public enum ReportStatus
{
    Pending = 0,
    Generating = 1,
    Ready = 2,
    Failed = 3
}
