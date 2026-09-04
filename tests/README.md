# MedClinic AI — Tests

## Structure

```
tests/
├── MedClinic.Tests.Unit/              # Fast unit tests (in-memory DB, no HTTP)
│   ├── Services/
│   │   ├── PatientServiceTests.cs     # 5 tests
│   │   ├── AppointmentServiceTests.cs # 4 tests
│   │   ├── AIServiceTests.cs          # 5 tests
│   │   ├── CanvasServiceTests.cs      # 4 tests
│   │   └── DentalServiceTests.cs      # 3 tests
│   └── Domain/
│       ├── DentalConditionsTests.cs   # 3 tests
│       ├── BodyRegionsTests.cs        # 3 tests
│       └── AnnotationTypesTests.cs    # 2 tests
│
└── MedClinic.Tests.Integration/       # Integration tests (WebApplicationFactory)
    ├── Fixtures/
    │   └── WebAppFactory.cs           # InMemory DB + Test env
    └── Controllers/
        ├── AuthControllerTests.cs     # 5 tests
        └── PatientsControllerTests.cs # 2 tests
```

## Running Tests

```bash
# Run all tests
dotnet test

# Unit tests only
dotnet test tests/MedClinic.Tests.Unit

# Integration tests only
dotnet test tests/MedClinic.Tests.Integration

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Coverage report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Test Summary

| Suite | Tests |
|---|---|
| Unit — Services | 21 |
| Unit — Domain | 8 |
| Integration | 7 |
| **Total** | **36** |

## Notes

- All unit tests use **InMemory EF Core** — no external dependencies.
- Integration tests use `WebApplicationFactory<Program>` with InMemory DB.
- `MockAIProvider` is used for AI tests — no real API key required.
- Tests are isolated: each test creates its own `DbContext` instance.
