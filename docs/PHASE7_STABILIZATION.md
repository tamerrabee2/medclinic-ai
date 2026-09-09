# Phase 7 — Stabilization & Production-Ready Checklist

## ✅ DbContext Registrations

All Phase 7 `DbSet<T>` properties are declared in:
- `IApplicationDbContext.Phase7.cs` (interface)
- `ApplicationDbContext.Phase7.cs` (implementation)

## ✅ EF Core Configurations

FluentAPI configurations exist for all entities:

| Entity | Config File |
|---|---|
| DicomStudy / DicomSeries / DicomInstance | `DicomStudyConfiguration.cs` |
| FhirSyncRecord | `FhirSyncRecordConfiguration.cs` |
| InsuranceProvider / Policy / Claim | `InsuranceConfiguration.cs` |
| ExternalLabProvider / Sync | `ExternalLabConfiguration.cs` |
| PatientPortalAccess / Message | `PatientPortalConfiguration.cs` |

## ✅ DI Registrations

All Phase 7 services are registered in `DependencyInjection.Phase7.cs`.

Call in `Program.cs`:
```csharp
builder.Services.AddPhase7Services(builder.Configuration);
```

## ✅ Master Migration

Run `Phase7_Master.sql` in PostgreSQL — idempotent (`CREATE TABLE IF NOT EXISTS`).

## ✅ Global Exception Handling

`GlobalExceptionHandler.cs` maps domain exceptions to RFC 7807 ProblemDetails.

Wire in `Program.cs`:
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();
```

## ✅ Result<T> + PaginatedList<T>

Shared `Result.cs` models standardize all command/query responses.

## 🔲 TODO for full production

- [ ] Replace MockPacsProvider with OrthancPacsProvider (`PACS_PROVIDER=Orthanc` env)
- [ ] Replace MockFhirProvider with Hl7.Fhir.R4 NuGet client
- [ ] Replace MockInsuranceProviderGateway with payer-specific adapter
- [ ] Replace MockExternalLabProvider with HL7 v2.x or lab-specific REST adapter
- [ ] Add EF Core code-first migration or sync DB via `Phase7_Master.sql`
- [ ] Add integration tests for each Phase 7 controller
- [ ] Rate-limit DICOM upload endpoint
- [ ] Encrypt `PatientPortalAccess.PasswordHash` with BCrypt at service layer
