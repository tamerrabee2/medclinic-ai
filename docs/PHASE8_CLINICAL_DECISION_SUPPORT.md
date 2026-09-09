# Phase 8.1–8.2 — Clinical Decision Support

## Scope delivered

- Differential-diagnosis suggestions based on a deliberately conservative, explainable baseline rules catalog.
- Drug–drug interaction checks against a small curated safety-rule catalog.
- Critical red-flag alerts for chest pain, dyspnea, and reported SpO2 below 90%.
- Authenticated REST endpoints under `/api/v1/patients/{patientId}/clinical-decision-support`.
- Explicit clinician-review requirement in every response.

## Startup registration

Register the service with the infrastructure container:

```csharp
builder.Services.AddPhase8Services();
```

## API examples

### Differential diagnosis

```http
POST /api/v1/patients/{patientId}/clinical-decision-support/differential-diagnosis
Content-Type: application/json

{
  "symptoms": ["chest pain", "shortness of breath"],
  "vitals": { "SpO2": "88" },
  "labResults": { "troponin": "pending" },
  "clinicalNotes": "New exertional pain"
}
```

### Drug interaction check

```http
POST /api/v1/patients/{patientId}/clinical-decision-support/drug-interactions
Content-Type: application/json

{
  "medications": [
    { "name": "Warfarin", "dose": "5 mg", "route": "oral" },
    { "name": "Ibuprofen", "dose": "400 mg", "route": "oral" }
  ]
}
```

## Safety and production requirements

This module is **clinical decision support**, not a diagnostic or prescribing system. It must not operate as an autonomous clinical decision maker.

Before production deployment:

1. Replace the baseline rules with a licensed, versioned medication knowledge source and locally approved clinical pathways.
2. Normalize medications to RxNorm (or the jurisdictional equivalent), including ingredients, dose, route, renal/hepatic adjustments, allergy history, and active versus discontinued status.
3. Persist request inputs, ruleset/version, returned alerts, clinician override/reason, reviewer identity, and timestamps to the audit trail.
4. Validate sensitivity, specificity, alert fatigue, latency, localization, and clinical governance with the clinic's medical leadership.
5. Apply role-based access control and emergency escalation policies.

The included catalog intentionally covers only a small set of well-known high-risk examples and is **not comprehensive**.
