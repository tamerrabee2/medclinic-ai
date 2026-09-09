# Consent Management API

Tenant-scoped authenticated consent API. Register `builder.Services.AddConsentManagement();`. Before an AI workflow uses patient data, call `HasActiveConsentAsync(patientId, ConsentType.AiAssistedCare)` according to clinic policy.