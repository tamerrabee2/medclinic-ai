# MedClinic AI — Work Plan & Product Blueprint

> **Your AI Clinical Workspace**
> مساحة العمل الطبية الذكية للطبيب

---

## 1. Product Vision

```
                    MedClinic AI
                         │
        ┌────────────────┼────────────────┐
        │                │                │
     Clinic           Clinical            AI
   Management         Workspace         Intelligence
        │                │                │
   Appointments       EMR             AI Assistant
   Billing            Visits          Lab Analysis
   Staff              Prescriptions   Imaging AI
   Reports            Timeline        Patient Summary
                       Canvas          Voice Scribe
                       Body Map        AI Search
```

The product is not merely a clinic management system.  
It is an **AI Clinical Workspace** — an operating system for the modern clinic with an AI Copilot working alongside the physician.

---

## 2. Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | C# / .NET 10 / ASP.NET Core Web API |
| **ORM** | Entity Framework Core |
| **Database** | PostgreSQL |
| **Cache / Jobs** | Redis + Hangfire (when needed) |
| **Frontend** | Next.js + React + TypeScript |
| **Runtime/Tooling** | Node.js |
| **UI Library** | Tailwind CSS + shadcn/ui |
| **State / Query** | TanStack Query |
| **Forms** | React Hook Form + Zod |
| **Icons** | Lucide |
| **Testing (BE)** | xUnit + FluentAssertions + Testcontainers |
| **Testing (FE)** | Vitest + React Testing Library + Playwright |
| **Architecture** | Clean Architecture + Modular + Multi-tenant |
| **Deployment** | Docker + GitHub Actions |

---

## 3. Architecture Overview

```
                    ┌──────────────────┐
                    │   Next.js Web    │
                    │ React + TS       │
                    └────────┬─────────┘
                             │
                         REST / SSE
                             │
                             ▼
                    ┌──────────────────┐
                    │ ASP.NET Core API │
                    │ .NET 10 / C#     │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
       Application        Domain      Infrastructure
              │              │              │
              └──────────────┼──────────────┘
                             │
                ┌────────────┼─────────────┐
                ▼            ▼             ▼
           PostgreSQL      Redis       Object Storage
                                          │
                                          ▼
                                    Medical Files
```

### Multi-Tenancy

Every Clinic = Tenant. Clinic A cannot access Clinic B's data.

### AI Architecture

```
                AI Gateway
                     │
        ┌────────────┼─────────────┐
        │            │             │
     Vision        Text          Voice
        │            │             │
        ▼            ▼             ▼
   Imaging AI    Clinical AI    Speech AI
```

**AI Provider abstraction:**

```csharp
public interface IAIProvider
{
    Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(...);
    Task<LabAnalysisResult> AnalyzeLabAsync(...);
    Task<PatientSummaryResult> SummarizePatientAsync(...);
    Task<ClinicalNoteResult> GenerateClinicalNoteAsync(...);
    Task<AIChatResponse> ChatAsync(...);
}
```

Supported providers:
- `MockAIProvider` (default / demo / zero-cost)
- `OpenAIProvider`
- `GeminiProvider`
- `AnthropicProvider`
- `LocalAIProvider`
- `CloudAIProvider`

> AI output **must always require physician review**. Never auto-approve diagnosis or prescriptions.

---

## 4. Repository Structure

```
medclinic-ai/
│
├── frontend/
├── src/
│   ├── MedClinic.API
│   ├── MedClinic.Application
│   ├── MedClinic.Domain
│   ├── MedClinic.Infrastructure
│   └── MedClinic.Shared
├── tests/
│   ├── MedClinic.UnitTests
│   ├── MedClinic.IntegrationTests
│   └── MedClinic.ArchitectureTests
├── infrastructure/
├── docs/
├── docker/
├── .github/workflows/
├── docker-compose.yml
├── README.md
├── ARCHITECTURE.md
├── SECURITY.md
├── CONTRIBUTING.md
├── WORK_PLAN.md
├── LICENSE
└── .gitignore
```

### Frontend Structure

```
frontend/
├── app/
├── components/
│   ├── ui/
│   ├── dashboard/
│   ├── patients/
│   ├── appointments/
│   ├── clinical/
│   ├── ai/
│   ├── imaging/
│   ├── laboratory/
│   ├── canvas/
│   └── body-map/
├── features/
├── hooks/
├── lib/
├── services/
├── types/
├── stores/
├── i18n/
└── public/
```

---

## 5. Database — Core Tables

```
Users / Roles / Permissions
Clinics / ClinicMembers
Doctors / Patients / PatientContacts
MedicalRecords / Visits / Diagnoses
Medications / Prescriptions / PrescriptionItems
Appointments
LabOrders / LabResults / LabResultItems
RadiologyStudies / MedicalImages
AIAnalyses / AIReports / AIConversations / AIConversationMessages
MedicalAnnotations / BodyAnnotations
Invoices / InvoiceItems / Payments
Notifications / AuditLogs
```

---

## 6. Pages Map

### Public
```
/
/features
/pricing
/security
/about
/contact
/login
/register
```

### Doctor Workspace
```
/dashboard
/patients
/patients/:id
/appointments
/clinical
/clinical/:visitId
/laboratory
/radiology
/imaging/:id
/ai
/prescriptions
/billing
/notifications
/settings
```

### Admin
```
/admin
/admin/clinics
/admin/users
/admin/doctors
/admin/reports
/admin/billing
/admin/settings
```

---

## 7. Key UX Features

- **Command Palette** (`Ctrl+K`) — search patients, open AI, new appointment...
- **AI Patient Brief** — context-aware summary before each visit
- **Interactive Medical Timeline** — full patient history in one view
- **Medical Canvas** — annotations layer on top of original image (never modify original)
- **Interactive Body Map** — SVG-based, clickable regions, add symptoms/notes per region
- **Dark Mode** — proper dark mode with medical-grade readability
- **RTL/LTR + Arabic/English i18n**
- **Responsive** — Desktop full workspace, Tablet collapsible sidebar, Mobile bottom nav
- **Skeleton loading, meaningful empty states, subtle animations**

### Design System Components
```
Button / Input / Select / DatePicker / Modal / Drawer
Tabs / Card / Table / Badge / Avatar / Tooltip
Dropdown / Toast / Skeleton / Command Palette
Timeline / Chart / Medical Card / AI Card / Patient Card
```

### Color Palette
| Token | Value |
|---|---|
| Background | White / Light Gray |
| Primary | Calm Medical Blue |
| Secondary | Teal |
| AI Accent | Purple / Indigo |
| Success | Green |
| Warning | Amber |
| Danger | Red |

Typography: **IBM Plex Sans Arabic** or **Noto Sans Arabic** with compatible Latin font.

---

## 8. AI Workflows (Embedded in Clinical Flow)

| Context | AI Action |
|---|---|
| Patient Profile | 🧠 Summarize Patient |
| Lab Results | 🧪 Explain Results |
| Medical Imaging | 🩻 Analyze Image |
| Visit Note | ✨ Draft Clinical Note |
| Timeline | 🧠 What changed? |
| Prescription | 💊 Review Context |

---

## 9. Medical Canvas — Tool Set

```
Pen / Brush / Arrow / Line / Rectangle / Circle
Polygon / Text / Measurement / Eraser
Undo / Redo / Zoom / Pan / Rotate
Brightness / Contrast / Invert
```

**Rule:** Never modify the original image. Store:
- Original Image
- Annotation JSON
- Annotated Preview

---

## 10. Dental Module (Phase 7)

```
      11 12 13 14 15 16
      21 22 23 24 25 26

      31 32 33 34 35 36
      41 42 43 44 45 46
```

Each tooth: Normal / Caries / Filling / Crown / Missing / Implant / Root Canal / Extraction

---

## 11. Voice AI (Phase 5)

```
Voice
 ↓
Speech-to-Text
 ↓
Clinical NLP
 ↓
Structured Note (Chief Complaint / HPI / Examination / Assessment / Plan)
 ↓
Doctor Review
```

---

## 12. Roadmap

### ✅ Phase 0 — Foundation
- Repository, Architecture, Docker, CI
- PostgreSQL, .NET 10, Next.js, Authentication

### ✅ Phase 1 — Core Clinic
- Clinics, Users, Roles, Doctors, Patients, Appointments

### ✅ Phase 2 — Clinical Workspace
- Patient profile, Medical record, Visits, Timeline, Prescriptions, Lab, Radiology

### ✅ Phase 3 — AI
- AI Gateway, Mock AI, AI Chat, Patient Summary, Lab Analysis, Imaging integration

### ✅ Phase 4 — Visual Clinical
- Medical Canvas, Image Viewer, Annotations, Body Map, Image comparison

### 🔲 Phase 5 — Intelligence
- Voice Scribe, AI Patient Brief, AI Clinical Timeline, AI-powered workflow, Follow-up intelligence

### 🔲 Phase 6 — Business
- Billing, Payments, Reports, Notifications, WhatsApp integration

### 🔲 Phase 7 — Advanced
- Dental Module, DICOM, PACS integration, FHIR, External labs, Insurance, Patient Portal

---

## 13. Testing Strategy

### Backend
- xUnit + FluentAssertions
- Integration Tests with WebApplicationFactory
- Testcontainers for PostgreSQL

### Frontend
- Vitest + React Testing Library
- Playwright for E2E

### Critical E2E Flow
```
Login → Create Clinic → Create Doctor → Create Patient
→ Create Appointment → Open Visit → Upload Lab
→ AI Analysis → Upload Image → Annotate
→ AI Summary → Doctor Approval → Prescription → Follow-up
```

---

## 14. DevOps

### Docker
```yaml
services:
  frontend
  backend
  postgres
  redis
  # local-ai (optional)
```

### GitHub Actions
```
.github/workflows/
  ci.yml
  backend.yml
  frontend.yml
  e2e.yml
  security.yml
```

Each PR runs: Restore → Build → Lint → Test → Security Scan → Frontend Build → Backend Build

---

## 15. Security Requirements

```
Authentication / Authorization / RBAC
Multi-tenancy + Tenant Isolation
Audit Logs / Encrypted Transport
Secure File Storage / Signed URLs
Rate Limiting / Input Validation
File Validation / Secret Management
```

> AI never receives data it doesn't need.  
> All AI results require physician review before being saved to medical records.

---

## 16. AI Safety Policy

```
AI GENERATED
       ↓
Doctor Review
       ↓
Approve / Edit
       ↓
Medical Record
```

**Never:**
```
AI → Automatic Diagnosis → Automatic Prescription
```

---

## 17. Master Prompt (for AI Agent)

```
You are the lead engineer responsible for building MedClinic AI.

Build a production-ready AI-powered clinical workspace for modern medical clinics.

DO NOT create a prototype.
DO NOT create fake buttons.
DO NOT create empty pages.
DO NOT leave critical features as TODO.
DO NOT hardcode secrets.

TECH STACK:
Backend: C# / .NET 10 / ASP.NET Core / EF Core / PostgreSQL / Redis
Frontend: Next.js / React / TypeScript / Tailwind CSS / shadcn/ui / TanStack Query
Architecture: Clean Architecture / Modular / Multi-tenant / REST API

EXECUTION STRATEGY:
Work phase by phase. After every phase:
1. Build
2. Run tests
3. Fix errors
4. Run lint + type checks
5. Commit changes
6. Continue

If a dependency or external AI provider requires credentials:
- create the abstraction
- create environment variables
- create MockAIProvider
- document the requirement
- never hardcode credentials

If a medical AI capability is not safely or technically available:
- do not fake the result
- create the correct integration boundary
- document the limitation

The final result must be a real, maintainable, scalable medical SaaS application.
```

---

*Last updated: September 2026*
