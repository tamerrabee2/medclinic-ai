using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Common.Interfaces;

public interface IApplicationDbContext : MedClinic.Application.Interfaces.IApplicationDbContext
{
    // ─── DICOM ─────────────────────────────────────────────────────────────────
    DbSet<DicomStudy> DicomStudies { get; }
    DbSet<DicomSeries> DicomSeriesList { get; }
    DbSet<DicomInstance> DicomInstances { get; }

    // ─── FHIR ──────────────────────────────────────────────────────────────────
    DbSet<FhirSyncRecord> FhirSyncRecords { get; }

    // ─── External Labs ─────────────────────────────────────────────────────────
    DbSet<ExternalLabProvider> ExternalLabProviders { get; }
    DbSet<ExternalLabSync> ExternalLabSyncs { get; }

    // ─── Insurance ─────────────────────────────────────────────────────────────
    DbSet<InsuranceProvider> InsuranceProviders { get; }
    DbSet<InsurancePolicy> InsurancePolicies { get; }
    DbSet<InsuranceClaim> InsuranceClaims { get; }

    // ─── Patient Portal ────────────────────────────────────────────────────────
    DbSet<PatientPortalAccess> PatientPortalAccesses { get; }
    DbSet<PatientPortalMessage> PatientPortalMessages { get; }

    // ─── Advanced Intelligence & Clinical ──────────────────────────────────────
    DbSet<VoiceNote> VoiceNotes { get; }
    DbSet<DentalChart> DentalCharts { get; }
    DbSet<ToothRecord> ToothRecords { get; }
    DbSet<PatientBrief> PatientBriefs { get; }
    DbSet<ClinicalTimelineEvent> ClinicalTimelineEvents { get; }
    DbSet<FollowUpIntelligence> FollowUpIntelligences { get; }
    DbSet<ClinicReport> ClinicReports { get; }
    DbSet<NotificationLog> NotificationLogs { get; }
}
