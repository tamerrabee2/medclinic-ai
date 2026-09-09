using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Persistence;

public partial class ApplicationDbContext
{
    // ─── DICOM ─────────────────────────────────────────────────────────────────
    public DbSet<DicomStudy> DicomStudies => Set<DicomStudy>();
    public DbSet<DicomSeries> DicomSeriesList => Set<DicomSeries>();
    public DbSet<DicomInstance> DicomInstances => Set<DicomInstance>();

    // ─── FHIR ──────────────────────────────────────────────────────────────────
    public DbSet<FhirSyncRecord> FhirSyncRecords => Set<FhirSyncRecord>();

    // ─── External Labs ─────────────────────────────────────────────────────────
    public DbSet<ExternalLabProvider> ExternalLabProviders => Set<ExternalLabProvider>();
    public DbSet<ExternalLabSync> ExternalLabSyncs => Set<ExternalLabSync>();

    // ─── Insurance ─────────────────────────────────────────────────────────────
    public DbSet<InsuranceProvider> InsuranceProviders => Set<InsuranceProvider>();
    public DbSet<InsurancePolicy> InsurancePolicies => Set<InsurancePolicy>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();

    // ─── Patient Portal ────────────────────────────────────────────────────────
    public DbSet<PatientPortalAccess> PatientPortalAccesses => Set<PatientPortalAccess>();
    public DbSet<PatientPortalMessage> PatientPortalMessages => Set<PatientPortalMessage>();
}
