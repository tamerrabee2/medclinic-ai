# 🔒 Security Policy — MedClinic AI

## Supported Versions

| Version | Supported |
|---|---|
| 1.x (current) | ✅ |

---

## Reporting a Vulnerability

If you discover a security vulnerability, please **do NOT** open a public GitHub issue.

📧 Contact: **security@medclinic.ai**

Please include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

We will respond within **48 hours** and issue a patch within **7 days** for critical vulnerabilities.

---

## Security Architecture

### Authentication
- **JWT Bearer Tokens** — RS256 signed, 60-minute expiry
- **Refresh Token Rotation** — single-use tokens, automatically rotated
- **Refresh Token Revocation** — invalidated on logout or admin action
- **Password Hashing** — BCrypt with cost factor 12
- **Brute Force Protection** — account lockout after 5 failed attempts

### Authorization
- **RBAC** — Role-Based Access Control with granular permissions
- **Policy-based** — every endpoint has an explicit `[Authorize(Policy)]`
- **Tenant Isolation** — enforced at service layer, all queries filtered by `ClinicId`

### Data Security
- **Encryption at Rest** — PostgreSQL transparent data encryption
- **Encryption in Transit** — TLS 1.3 enforced
- **PHI Protection** — all patient data scoped to authenticated clinic context
- **SQL Injection Prevention** — EF Core parameterized queries exclusively
- **XSS Prevention** — input sanitization + Content Security Policy headers

### Medical AI Safety
- All AI outputs are marked `RequiresDoctorReview: true`
- AI responses always contain a medical disclaimer
- No AI system can trigger clinical actions autonomously
- AI conversations are scoped to the authenticated user's clinic

### Audit & Compliance
- **Immutable Audit Logs** — every data mutation is permanently recorded
- **HIPAA-aligned** — access logs, PHI handling, minimum necessary access
- **GDPR-ready** — data export, right to deletion (soft delete)
- **Audit fields** — all entities have `CreatedAt`, `UpdatedAt`, `CreatedBy`

### Infrastructure
- **Secrets Management** — environment variables / Azure Key Vault (never in source code)
- **CORS Policy** — whitelist only; no wildcard `*` in production
- **Rate Limiting** — per-IP and per-user limits on sensitive endpoints
- **Security Headers** — HSTS, X-Frame-Options, X-Content-Type-Options

---

## Known Limitations

- AI analysis is provided by third-party providers (OpenAI). Patient data sent to AI must comply with your organization's data processing agreements.
- In development mode (`AI:Provider=Mock`), no data leaves the system.

---

## Security Checklist for Deployment

- [ ] Replace all default secrets in `appsettings.json`
- [ ] Set `AI:Provider=OpenAI` only after reviewing your data processing agreement
- [ ] Enable HTTPS-only (`ASPNETCORE_HTTPS_PORTS`)
- [ ] Configure CORS to your frontend domain only
- [ ] Set up database backups and point-in-time recovery
- [ ] Enable PostgreSQL SSL connections
- [ ] Review and harden Docker network policies
- [ ] Enable application-level rate limiting
