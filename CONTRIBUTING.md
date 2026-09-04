# Contributing to MedClinic AI

Thank you for your interest in contributing!

## Development Setup

1. Fork the repository
2. Clone your fork
3. Create a feature branch: `git checkout -b feat/your-feature`
4. Commit with conventional commits: `feat:`, `fix:`, `docs:`, `chore:`, `test:`
5. Push and open a Pull Request

## Code Standards

### Backend (C#)
- Follow Clean Architecture layers
- No business logic in Controllers
- Use `async/await` and `CancellationToken` everywhere
- Use Dependency Injection
- Never hardcode secrets or configuration
- Write unit tests for all new features

### Frontend (TypeScript)
- Use TypeScript strictly (no `any`)
- Use React Server Components where appropriate
- Validate all inputs with Zod
- Handle loading, error, and empty states

## Pull Request Checklist

- [ ] Tests pass (`dotnet test` / `npm run test`)
- [ ] No secrets committed
- [ ] Follows code standards
- [ ] Documentation updated if needed
- [ ] No patient data in commits
