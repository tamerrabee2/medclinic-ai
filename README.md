# 🏥 MedClinic AI

> AI-powered Medical Clinic Management Platform

[![Build](https://github.com/tamerrabee2/medclinic-ai/actions/workflows/ci.yml/badge.svg)](https://github.com/tamerrabee2/medclinic-ai/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## Overview

MedClinic AI is a modern SaaS platform for managing medical clinics, doctors, staff, patients, appointments, EMR, lab results, radiology, AI-powered analysis, billing, and more.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / .NET 10 / ASP.NET Core Web API |
| Database | PostgreSQL + Entity Framework Core |
| Frontend | Next.js 15 / React / TypeScript / Tailwind CSS / shadcn/ui |
| AI | IAIProvider abstraction (OpenAI / Gemini / Local / Mock) |
| Cache | Redis |
| Background Jobs | Hangfire |
| Auth | ASP.NET Core Identity + JWT + Refresh Tokens |
| Docs | Swagger / OpenAPI |
| CI/CD | GitHub Actions |
| Containers | Docker + docker-compose |

## Quick Start

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16 (or use Docker)
- Redis (or use Docker)

### 1. Clone the repository

```bash
git clone https://github.com/tamerrabee2/medclinic-ai.git
cd medclinic-ai
```

### 2. Setup environment variables

```bash
cp .env.example .env
# Edit .env and fill in your values
```

### 3. Run with Docker Compose (recommended)

```bash
docker-compose up --build
```

This starts:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

### 4. Run without Docker

#### Backend

```bash
cd src/MedClinic.API
dotnet restore
dotnet ef database update
dotnet run
```

#### Frontend

```bash
cd frontend
npm install
npm run dev
```

## Database Migrations

```bash
cd src/MedClinic.API
dotnet ef migrations add InitialCreate --project ../MedClinic.Infrastructure
dotnet ef database update
```

## Running Tests

```bash
# Backend unit tests
dotnet test tests/MedClinic.UnitTests

# Backend integration tests
dotnet test tests/MedClinic.IntegrationTests

# Frontend tests
cd frontend && npm run test

# E2E tests
cd frontend && npm run test:e2e
```

## AI Configuration

The platform supports multiple AI providers. Set `AI_PROVIDER` in `.env`:

| Value | Description |
|---|---|
| `mock` | Default for development — no API key needed |
| `local` | Local AI model via Ollama |
| `openai` | OpenAI GPT-4o / Vision |
| `gemini` | Google Gemini |
| `anthropic` | Anthropic Claude |

> ⚠️ **Medical AI Disclaimer**: All AI-generated content is for clinical decision support only and must be reviewed by a qualified healthcare professional.

## Environment Variables

See [.env.example](.env.example) for all required variables.

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation.

## Security

See [SECURITY.md](SECURITY.md) for security policy and vulnerability reporting.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)
