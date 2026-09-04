# 🏗️ Architecture — MedClinic AI

## Overview

MedClinic AI follows **Clean Architecture** (also known as Onion Architecture), ensuring:
- Business logic is independent of frameworks and infrastructure
- Each layer depends only on inner layers
- Easy testability through dependency inversion

```
┌─────────────────────────────────────────────────────┐
│                    API Layer                        │
│          (Controllers, Middleware, Filters)         │
├─────────────────────────────────────────────────────┤
│                Application Layer                    │
│        (Services, DTOs, Use Cases, Interfaces)      │
├─────────────────────────────────────────────────────┤
│                  Domain Layer                       │
│           (Entities, Value Objects, Rules)          │
├─────────────────────────────────────────────────────┤
│              Infrastructure Layer                   │
│    (EF Core, AI Providers, File Storage, Email)     │
└─────────────────────────────────────────────────────┘
```

---

## Multi-Tenancy

MedClinic AI uses **row-level multi-tenancy**: all tenant data shares the same database schema, isolated by `ClinicId`.

```
Request → JWT → TenantMiddleware → ITenantContext (ClinicId, UserId)
                                       ↓
                              All queries automatically
                              filtered by ClinicId via
                              ITenantContext injection
```

**Key rules:**
- Every entity that belongs to a clinic has a `ClinicId` column
- Services receive `ITenantContext` and filter all queries with `.Where(e => e.ClinicId == _tenant.ClinicId)`
- No tenant can ever access another tenant's data — enforced at service layer, not controller

---

## Authentication & Authorization

```
Login → JWT Access Token (60 min) + Refresh Token (7 days)
     → Stored in HttpOnly Cookie OR Authorization header
     → Refresh Token Rotation on every use
     → Revocable via logout or admin action
```

### Permissions
Permissions are defined as policy strings in `MedClinic.Shared.Constants.Permissions`:

```csharp
public static class Permissions
{
    public const string PatientsRead      = "patients:read";
    public const string PatientsCreate    = "patients:create";
    public const string MedicalRecordsRead = "medical:read";
    public const string AIAnalysis        = "ai:analysis";
    // ...
}
```

Each role is assigned a set of permissions. The `[Authorize(Policy = Permissions.X)]` attribute enforces them at the endpoint level.

---

## Database Design

- **ORM:** Entity Framework Core 9 with PostgreSQL (Npgsql)
- **Migrations:** Code-first
- **Configuration:** Fluent API in `*Configuration.cs` files (no data annotations on entities)
- **Tables:** 28+ entities with proper indexes and foreign key constraints

### Key Relationships
```
Clinic ──< Doctor ──< Appointment
       ──< Patient ──< Visit ──< Prescription
                            ──< LabOrder ──< LabResult
                            ──< RadiologyStudy ──< RadiologyImage
       ──< Invoice ──< Payment
       ──< AIConversation ──< AIMessage
```

---

## AI Architecture

```
IAIProvider (interface)
    ├── MockAIProvider      ← Dev/Test: no external API, realistic responses
    └── OpenAIProvider      ← Prod: GPT-4o with Vision support
```

The provider is injected via DI and configured through `appsettings.json`:
```json
{ "AI": { "Provider": "Mock" } }   // or "OpenAI"
```

All AI responses include:
- `RequiresDoctorReview: true` — always enforced
- `Disclaimer` — legal disclaimer text
- Structured data (not just raw text)

---

## Medical Canvas

```
Original Image (never modified)
     ↓
Canvas Tool (Frontend) → POST /api/v1/canvas/save
     ↓
  ┌─────────────────────────────────┐
  │  MedicalAnnotations table       │
  │  (Type, CoordinatesJSON, Color) │
  │  + Annotated preview image      │
  │    stored in File Storage       │
  └─────────────────────────────────┘
```

**Annotation Types:** Pen, Brush, Arrow, Line, Rectangle, Circle, Text, Measurement, Region

---

## Dental Module (FDI Notation)

```
  Upper Right │ Upper Left
  18 17 16 15 14 13 12 11 │ 21 22 23 24 25 26 27 28
  ──────────────────────────────────────────────────
  48 47 46 45 44 43 42 41 │ 31 32 33 34 35 36 37 38
  Lower Right │ Lower Left
```

Each tooth record stores: `ToothNumber`, `Condition`, `Surface`, `TreatmentDate`  
Conditions: Healthy, Cavity, Filling, Crown, Missing, Implant, RootCanal, Extraction, Fracture, Veneer, Bridge

---

## File Storage

The `IFileStorage` interface abstracts file operations:
```csharp
public interface IFileStorage
{
    Task<string> UploadAsync(string path, Stream stream, string mimeType, CancellationToken ct);
    Task<Stream> DownloadAsync(string path, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}
```

**Implementations:**
- `LocalFileStorage` — Development
- `S3FileStorage` — Production (AWS S3 / MinIO)

---

## Audit Logging

Every data mutation (Create, Update, Delete) is automatically logged:
```
AuditLog { EntityName, EntityId, Action, OldValues, NewValues, UserId, ClinicId, Timestamp }
```
Logs are **immutable** — no UPDATE or DELETE is ever performed on audit records.

---

## Error Handling

Global exception handling via middleware:
```
Exception Type          → HTTP Status
KeyNotFoundException    → 404 Not Found
UnauthorizedAccessException → 403 Forbidden
InvalidOperationException → 400 Bad Request
ValidationException     → 422 Unprocessable Entity
Other                   → 500 Internal Server Error
```

All error responses follow:
```json
{
  "success": false,
  "error": "Error message",
  "code": "ERROR_CODE"
}
```
