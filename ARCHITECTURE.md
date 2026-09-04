# MedClinic AI — Architecture

## High-Level Overview

```
Browser / Mobile / Desktop
         ↓
    Next.js Frontend
         ↓
  ASP.NET Core Web API  ←→  Redis (Cache / Rate Limiting / Sessions)
         ↓
   Application Layer    ←→  Hangfire (Background Jobs)
         ↓
    Domain Layer
         ↓
  Infrastructure Layer  ←→  AI Providers (OpenAI / Gemini / Local / Mock)
         ↓               ←→  File Storage (Local / S3 / Azure Blob)
      PostgreSQL         ←→  Email / Notifications
```

## Project Structure

```
medclinic-ai/
├── src/
│   ├── MedClinic.API/              # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   ├── Extensions/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── MedClinic.Application/      # Application Layer (CQRS / Use Cases)
│   │   ├── Common/
│   │   ├── DTOs/
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   ├── Clinics/
│   │   │   ├── Doctors/
│   │   │   ├── Patients/
│   │   │   ├── Appointments/
│   │   │   ├── MedicalRecords/
│   │   │   ├── Prescriptions/
│   │   │   ├── Laboratory/
│   │   │   ├── Radiology/
│   │   │   ├── AI/
│   │   │   ├── Billing/
│   │   │   └── Notifications/
│   │   ├── Interfaces/
│   │   └── Services/
│   │
│   ├── MedClinic.Domain/           # Domain Layer (Entities, Value Objects, Events)
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Exceptions/
│   │
│   ├── MedClinic.Infrastructure/   # Infrastructure (EF Core, AI, Storage, Jobs)
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   ├── AI/
│   │   │   ├── IAIProvider.cs
│   │   │   ├── MockAIProvider.cs
│   │   │   ├── OpenAIProvider.cs
│   │   │   ├── GeminiProvider.cs
│   │   │   └── LocalAIProvider.cs
│   │   ├── Storage/
│   │   ├── Notifications/
│   │   ├── BackgroundJobs/
│   │   └── ExternalServices/
│   │
│   └── MedClinic.Shared/           # Shared Kernel
│
├── tests/
│   ├── MedClinic.UnitTests/
│   ├── MedClinic.IntegrationTests/
│   └── MedClinic.E2ETests/
│
├── frontend/                       # Next.js Frontend
│   ├── app/
│   ├── components/
│   ├── lib/
│   └── ...
│
├── .github/
│   └── workflows/
│       ├── ci.yml
│       ├── backend-tests.yml
│       └── frontend-tests.yml
│
├── docker-compose.yml
└── Dockerfile
```

## Multi-Tenancy

Each Clinic is an isolated Tenant. Tenant isolation is enforced at:
- **API Layer**: TenantId extracted from JWT claims
- **Application Layer**: All queries filtered by TenantId via ITenantContext
- **Database Layer**: Global Query Filters on all tenant-scoped entities

## AI Architecture

All AI functionality is abstracted behind `IAIProvider`. Business logic never calls AI providers directly.

```csharp
public interface IAIProvider
{
    Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(...);
    Task<LabAnalysisResult> AnalyzeLabResultsAsync(...);
    Task<PatientSummaryResult> SummarizePatientAsync(...);
    Task<MedicalReportResult> GenerateMedicalReportAsync(...);
    Task<AIChatResponse> ChatAsync(...);
}
```

Configured at startup via `AI_PROVIDER` environment variable.

## RBAC & Permissions

Roles: `SuperAdmin`, `ClinicAdmin`, `Doctor`, `Nurse`, `Receptionist`, `LabTechnician`, `Radiologist`, `Accountant`, `Patient`

Fine-grained permissions (e.g., `Patients.Read`, `MedicalRecords.Create`, `AI.Analysis`) enforced via policy-based authorization.

## API Versioning

All endpoints versioned under `/api/v1/...`

## Observability

- Structured logging via Serilog
- Correlation IDs on every request
- Health checks at `/health`, `/health/ready`, `/health/live`
- No sensitive medical data in logs
