# MedClinic AI — Project Roadmap

> Last updated: September 2026  
> Current status: **Phase 7 Complete ✅**

---

## ✅ Completed Phases

| Phase | Title | Status |
|-------|-------|--------|
| Phase 1 | Project Foundation & Architecture | ✅ Done |
| Phase 2 | Core Domain Entities | ✅ Done |
| Phase 3 | Patient Management & Registration | ✅ Done |
| Phase 4 | Appointments & Scheduling | ✅ Done |
| Phase 5 | Clinical Records, Lab Orders & Prescriptions | ✅ Done |
| Phase 6 | Billing, Invoicing & Payments | ✅ Done |
| Phase 7 | Integrations: Dental, DICOM/PACS, FHIR, External Labs, Insurance, Patient Portal | ✅ Done |

---

## 🔮 Upcoming Phases

---

## Phase 8 — Advanced AI & Clinical Decision Support

> **Priority: HIGH** | Estimated effort: 3–4 weeks

### Goals
Elevate the AI layer from assistive to proactive clinical intelligence.

### Deliverables

| # | Feature | Details |
|---|---------|--------|
| 8.1 | Differential Diagnosis Engine | AI suggests ranked differentials from symptoms + vitals + lab results |
| 8.2 | Drug–Drug Interaction Checker | Real-time alert when prescribing conflicting medications |
| 8.3 | Predictive Risk Scoring | Readmission risk, deterioration score, chronic disease progression |
| 8.4 | Clinical NLP — Visit Summarization | Auto-generate structured SOAP note from free-text visit notes |
| 8.5 | AI Triage Assistant | Priority scoring for incoming patients based on symptoms |
| 8.6 | Radiology Report Generation | Structured report draft from DICOM AI findings (Phase 7 bridge) |
| 8.7 | AI Safety Guardrails v2 | Confidence thresholds, mandatory doctor approval workflow, audit trail |

### Tech Stack Additions
- `Hl7.Fhir.R4` NuGet for structured clinical data export
- OpenAI / Azure OpenAI for NLP summarization
- Rule-engine (MedlinePlus or custom) for drug interactions

---

## Phase 9 — Multi-Tenancy, SaaS & Clinic Network

> **Priority: HIGH** | Estimated effort: 3–4 weeks

### Goals
Enable the system to serve multiple independent clinics (and clinic chains) from one deployment.

### Deliverables

| # | Feature | Details |
|---|---------|--------|
| 9.1 | Tenant Isolation Hardening | Row-level security per ClinicId in all queries + DB policies |
| 9.2 | Super-Admin Dashboard | Cross-tenant monitoring: usage, billing, active users |
| 9.3 | Clinic Onboarding Wizard | Self-service signup with plan selection and data seeding |
| 9.4 | Subscription & Billing Engine | Plan tiers (Basic / Pro / Enterprise), usage-based metering |
| 9.5 | Clinic Network / Referral System | Refer patients between clinics, shared lab results across network |
| 9.6 | White-Label Support | Per-tenant logo, colors, domain, email templates |
| 9.7 | Data Export & Portability | GDPR-compliant patient data export (JSON / FHIR bundle) |

### Tech Stack Additions
- Stripe / Paddle for subscription billing
- Outbox pattern for cross-tenant events
- Custom domain routing (wildcard DNS)

---

## Phase 10 — Mobile Application (React Native / Expo)

> **Priority: MEDIUM** | Estimated effort: 4–5 weeks

### Goals
Provide native iOS & Android apps for doctors, nurses, and patients.

### Deliverables

| # | Feature | Details |
|---|---------|--------|
| 10.1 | Doctor App | Patient list, visit notes, lab results, prescriptions on mobile |
| 10.2 | Patient App | Appointments, medical history, bills, portal messages, DICOM thumbnails |
| 10.3 | Nurse / Reception App | Check-in, vitals entry, triage queue management |
| 10.4 | Push Notifications | Appointment reminders, lab result alerts, prescription ready |
| 10.5 | Offline Support | Read-only cached data when no connectivity; sync on reconnect |
| 10.6 | Biometric Auth | Face ID / Touch ID for fast secure login |
| 10.7 | AI Chat in App | Floating AI assistant for quick queries and symptom checks |

### Tech Stack Additions
- React Native + Expo SDK
- Expo Notifications + FCM/APNs
- WatermelonDB or MMKV for offline cache

---

## Phase 11 — Analytics, BI & Reporting

> **Priority: MEDIUM** | Estimated effort: 2–3 weeks

### Goals
Give clinic administrators and doctors actionable data insights.

### Deliverables

| # | Feature | Details |
|---|---------|--------|
| 11.1 | Executive Dashboard | Revenue, appointments, new patients KPIs with trend charts |
| 11.2 | Clinical Analytics | Most common diagnoses, lab result trends, prescription patterns |
| 11.3 | Financial Reports | Collections, outstanding invoices, insurance claim approval rates |
| 11.4 | Appointment Analytics | No-show rate, peak hours, doctor utilization per specialty |
| 11.5 | AI Usage Audit Report | All AI suggestions generated, approved vs rejected by doctors |
| 11.6 | Custom Report Builder | Drag-and-drop report designer with export to PDF / Excel |
| 11.7 | Population Health View | Aggregate patient cohort analysis (diabetics, hypertensives, etc.) |

### Tech Stack Additions
- Apache ECharts or Recharts for in-app charts
- Optional: embedded Metabase or Superset for advanced BI
- PDF generation (QuestPDF or PuppeteerSharp)

---

## Phase 12 — Compliance, Security & Audit

> **Priority: HIGH** | Estimated effort: 2–3 weeks

### Goals
Reach HIPAA / GDPR / ISO 27001 alignment and harden the system for enterprise healthcare.

### Deliverables

| # | Feature | Details |
|---|---------|--------|
| 12.1 | Full Audit Log | Every create/update/delete action stored with actor, timestamp, diff |
| 12.2 | RBAC v2 | Fine-grained permissions per resource (view/edit/delete/approve) |
| 12.3 | Data Masking | PII masking in logs and export for non-privileged roles |
| 12.4 | Encryption at Rest | Sensitive fields (SSN, PasswordHash, payment info) encrypted in DB |
| 12.5 | MFA / 2FA | TOTP / SMS second factor for all privileged users |
| 12.6 | Pen Test Readiness | Input sanitization audit, SQL injection, XSS, CSRF review |
| 12.7 | Consent Management | Patient consent records for data use, AI analysis, sharing |
| 12.8 | Data Retention Policies | Configurable auto-archival and legal hold per tenant |

### Tech Stack Additions
- MediatR pipeline behavior for AuditLog
- `Microsoft.AspNetCore.DataProtection` for field-level encryption
- TOTP library (`OtpNet`)

---

## Suggested Execution Order

```
Phase 8  ───> Phase 12  ───> Phase 9
   (AI)      (Security)    (SaaS)
                  ↓
            Phase 11  ───> Phase 10
           (Analytics)    (Mobile)
```

**Rationale:**
- Ship AI first — it is the core product differentiator.
- Harden security before opening multi-tenancy.
- Add analytics once data volume is sufficient.
- Mobile app comes last as it depends on stable APIs.

---

## Total Remaining Estimate

| Phase | Weeks |
|-------|-------|
| Phase 8  — Advanced AI | 3–4 |
| Phase 9  — SaaS / Multi-Tenancy | 3–4 |
| Phase 10 — Mobile App | 4–5 |
| Phase 11 — Analytics & BI | 2–3 |
| Phase 12 — Compliance & Audit | 2–3 |
| **Total** | **14–19 weeks** |

---

## Contributing

Each phase begins with a feature branch `feat/phase-X` and merges to `main` via PR after:
1. Unit tests passing
2. AI safety review (for clinical features)
3. Stabilization pass (DI, DbContext, EF configs)
