# Security Policy

## Reporting a Vulnerability

Please do **not** open a public GitHub issue for security vulnerabilities.

Contact: security@medclinic-ai.example.com

We will respond within 72 hours.

## Security Measures

- HTTPS enforced
- JWT + Refresh Token rotation
- Password hashing (ASP.NET Core Identity / Argon2)
- RBAC + fine-grained permissions
- Multi-tenant isolation at DB level
- Rate limiting
- Input validation (FluentValidation)
- File upload validation (extension + MIME + signature + size)
- Secure file access via signed URLs / protected endpoints
- No secrets in source code
- CORS policy
- Secure HTTP headers
- SQL injection protection via EF Core parameterization
- XSS protection
- No stack traces exposed to clients
- Audit logging for all sensitive operations
- No sensitive medical data in application logs

## Medical Data

This platform handles sensitive medical information (PHI). Always ensure:
- Encryption in transit (TLS 1.2+)
- Encryption at rest where infrastructure supports it
- Minimal data exposure in APIs
- Patient data never appears in logs
- AI-generated content always requires doctor review
