# Phase 8.3–8.6 — Advanced Clinical AI Baseline

## Delivered capabilities

| Capability | Endpoint | Output |
|---|---|---|
| Predictive risk scoring | `POST /api/v1/patients/{patientId}/clinical-ai/risk-score` | Score, risk band, explainable contributions |
| SOAP note draft | `POST /api/v1/patients/{patientId}/clinical-ai/soap-draft` | Editable S/O/A/P draft |
| AI triage | `POST /api/v1/patients/{patientId}/clinical-ai/triage` | Routine/Urgent/Emergency assistive assessment |
| Radiology report draft | `POST /api/v1/patients/{patientId}/clinical-ai/radiology-report-draft` | Draft based only on provided findings/impressions |

## Startup registration

```csharp
builder.Services.AddPhase8AdvancedClinicalAIServices();
```

## Guardrails

- This is a conservative, deterministic baseline—not a clinically validated prediction model.
- Every output includes a mandatory clinician-review requirement.
- Triage results are never autonomous disposition decisions; emergency paths explicitly direct users to local escalation protocols.
- The radiology generator does not interpret images. It formats only supplied findings and requires radiologist sign-off.
- The SOAP generator does not invent undocumented facts; it retains free text when structured headings are absent.

## Production gates

Before clinical deployment, require medical governance approval, model/ruleset versioning, prospective validation, monitoring for bias and alert fatigue, full audit logging (including overrides), RBAC, and locally approved emergency policies. Use validated models and a suitably governed data pipeline; do not rely on the baseline rules for clinical decision-making.
