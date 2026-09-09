using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Common.Interfaces;

// Phase 7 DbSet extensions — partial interface extension
public partial interface IApplicationDbContext
{
    // DICOM
    DbSet<DicomStudy> DicomStudies { get; }
    DbSet<DicomSeries> DicomSeriesList { get; }
    DbSet<DicomInstance> DicomInstances { get; }

    // FHIR
    DbSet<FhirSyncRecord> FhirSyncRecords { get; }

    // External Labs
    DbSet<ExternalLabProvider> ExternalLabProviders { get; }
    DbSet<ExternalLabSync> ExternalLabSyncs { get; }

    // Insurance
    DbSet<InsuranceProvider> InsuranceProviders { get; }
    DbSet<InsurancePolicy> InsurancePolicies { get; }
    DbSet<InsuranceClaim> InsuranceClaims { get; }

    // Patient Portal
    DbSet<PatientPortalAccess> PatientPortalAccesses { get; }
    DbSet<PatientPortalMessage> PatientPortalMessages { get; }
}
