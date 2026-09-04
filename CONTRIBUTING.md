# 🤝 Contributing to MedClinic AI

Thank you for your interest in contributing!

---

## Development Setup

```bash
git clone https://github.com/tamerrabee2/medclinic-ai.git
cd medclinic-ai
dotnet restore
dotnet build
dotnet test
```

---

## Branch Strategy

```
main          ← stable, production-ready
dev           ← integration branch
feat/*        ← new features
fix/*         ← bug fixes
docs/*        ← documentation only
test/*        ← test additions
```

---

## Commit Convention

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(module): short description
fix(module): short description
docs: update README
test(module): add tests for X
refactor(module): extract service
chore: update dependencies
```

---

## Code Standards

- Follow **Clean Architecture** — no business logic in controllers
- All new endpoints must have an `[Authorize(Policy = Permissions.X)]` attribute
- Every new service method must have at least one unit test
- Use `ITenantContext` for all tenant-scoped queries — never accept `clinicId` from request body
- All AI outputs must include `RequiresDoctorReview = true` and a `Disclaimer`

---

## Adding a New Feature

1. Create entity in `MedClinic.Domain/Entities/`
2. Add EF configuration in `MedClinic.Infrastructure/Persistence/Configurations/`
3. Register `DbSet<T>` in `ApplicationDbContext`
4. Add DTOs in `MedClinic.Application/Features/{Feature}/DTOs/`
5. Implement service in `MedClinic.Application/Features/{Feature}/Services/`
6. Add controller in `MedClinic.API/Controllers/`
7. Register service in DI
8. Add unit tests in `tests/MedClinic.Tests.Unit/`
9. Run `dotnet ef migrations add {MigrationName}`

---

## Pull Request Checklist

- [ ] Tests pass (`dotnet test`)
- [ ] New feature has unit tests
- [ ] No hardcoded `ClinicId` or `UserId` — use `ITenantContext`
- [ ] No sensitive data in logs
- [ ] Audit logging added for mutations
- [ ] SECURITY.md updated if security-relevant
