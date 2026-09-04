# 🏥 MedClinic AI

> **AI-powered Multi-Tenant Clinic Management System**  
> Built with .NET 9 · Clean Architecture · PostgreSQL · JWT · Real-time Notifications

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-36%20passing-brightgreen?style=flat-square)](#running-tests)

---

## ✨ Features

| Module | Description |
|---|---|
| 🔐 **Auth & RBAC** | JWT + Refresh Tokens, Role-based permissions, 2FA ready |
| 🏢 **Multi-Tenant** | Full clinic isolation, per-tenant settings & branding |
| 👤 **Patients** | EMR, medical history, allergies, vital signs |
| 📅 **Appointments** | Scheduling, double-booking prevention, reminders |
| 🩺 **Visits & EMR** | SOAP notes, diagnoses, procedures, attachments |
| 💊 **Prescriptions** | Drug interactions check, dosage, refills |
| 🧪 **Laboratory** | Orders, results, trend analysis |
| 🩻 **Radiology** | DICOM-ready studies, AI image analysis |
| 🎨 **Medical Canvas** | Annotate images with Pen, Arrow, Measurement tools |
| 🧍 **Body Map** | Interactive SVG body with 26 clickable regions |
| 🦷 **Dental Module** | FDI tooth chart, conditions with color coding |
| 💰 **Billing** | Invoices, payments, multi-currency, reports |
| 🤖 **AI Assistant** | Dr. AI chat, Lab analysis, Patient summary, Image analysis |
| 🔔 **Notifications** | Real-time via SignalR, Email, SMS |
| 📊 **Analytics** | Dashboard KPIs, revenue trends, patient demographics |
| 📋 **Audit Logs** | Full immutable audit trail for HIPAA/GDPR compliance |

---

## 🏗️ Architecture

```
MedClinic AI
├── src/
│   ├── MedClinic.API              # ASP.NET Core 9 Web API (25 controllers, 130+ endpoints)
│   ├── MedClinic.Application      # Use cases, services, DTOs, interfaces
│   ├── MedClinic.Domain           # Entities, value objects, domain logic
│   ├── MedClinic.Infrastructure   # EF Core, Repositories, AI Providers, Storage
│   └── MedClinic.Shared           # Constants, helpers, cross-cutting concerns
└── tests/
    ├── MedClinic.Tests.Unit        # 29 unit tests (xUnit + FluentAssertions)
    └── MedClinic.Tests.Integration # 7 integration tests (WebApplicationFactory)
```

> See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed design decisions.

---

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/)
- [Redis](https://redis.io/) (optional, for caching)

### 1. Clone
```bash
git clone https://github.com/tamerrabee2/medclinic-ai.git
cd medclinic-ai
```

### 2. Configure
```bash
cp src/MedClinic.API/appsettings.example.json src/MedClinic.API/appsettings.json
```

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=medclinic;Username=postgres;Password=your_password"
  },
  "JWT": {
    "SecretKey": "your-super-secret-key-min-32-chars",
    "Issuer": "MedClinic",
    "Audience": "MedClinicAPI",
    "ExpiryMinutes": 60
  },
  "AI": {
    "Provider": "Mock",
    "OpenAI": {
      "ApiKey": "sk-...",
      "Model": "gpt-4o",
      "MaxTokens": 2048
    }
  }
}
```

### 3. Run Migrations
```bash
cd src/MedClinic.API
dotnet ef database update
```

### 4. Start
```bash
dotnet run --project src/MedClinic.API
```

API available at: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

---

## 🤖 AI Configuration

| Provider | Config Key | Notes |
|---|---|---|
| `Mock` | `AI:Provider=Mock` | Default. No API key needed. |
| `OpenAI` | `AI:Provider=OpenAI` | Requires `AI:OpenAI:ApiKey` |

> ⚠️ All AI outputs are **for clinical decision support only** and must be reviewed by a qualified healthcare professional.

---

## 🧪 Running Tests

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Unit tests only
dotnet test tests/MedClinic.Tests.Unit

# Integration tests only
dotnet test tests/MedClinic.Tests.Integration
```

Test results: **36 tests** (29 unit + 7 integration)

---

## 📡 API Overview

| Category | Endpoints |
|---|---|
| Auth | `/api/v1/auth` |
| Clinics | `/api/v1/clinics` |
| Patients | `/api/v1/patients` |
| Appointments | `/api/v1/appointments` |
| Visits / EMR | `/api/v1/visits` |
| Prescriptions | `/api/v1/prescriptions` |
| Laboratory | `/api/v1/lab-orders`, `/api/v1/lab-results` |
| Radiology | `/api/v1/radiology` |
| Medical Canvas | `/api/v1/canvas` |
| Billing | `/api/v1/invoices`, `/api/v1/payments` |
| AI Assistant | `/api/v1/ai` |
| Notifications | `/api/v1/notifications` |
| Analytics | `/api/v1/analytics`, `/api/v1/dashboard` |

Full API Reference: [API_REFERENCE.md](API_REFERENCE.md)

---

## 🔒 Security

- JWT Bearer authentication with refresh token rotation
- Role-based access control (RBAC) with granular permissions
- Full tenant isolation — data never leaks across clinics
- Immutable audit logs for every data mutation
- Input validation + SQL injection protection (parameterized EF Core)

See [SECURITY.md](SECURITY.md) for full security policy.

---

## 📄 License

MIT License — see [LICENSE](LICENSE)

---

<p align="center">
  Built with ❤️ for healthcare professionals
</p>
