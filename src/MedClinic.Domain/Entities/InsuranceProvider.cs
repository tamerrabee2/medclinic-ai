using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class InsuranceProvider : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PayerCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Clinic Clinic { get; set; } = null!;
}

public class InsurancePolicy : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid InsuranceProviderId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public decimal CoveragePercentage { get; set; }
    public bool IsPrimary { get; set; } = true;

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public InsuranceProvider Provider { get; set; } = null!;
}

public class InsuranceClaim : BaseAuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid InsurancePolicyId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public decimal ClaimedAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public string PayloadJson { get; set; } = "{}";
    public string? ResponseJson { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}

public enum ClaimStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    PartiallyApproved = 4
}
