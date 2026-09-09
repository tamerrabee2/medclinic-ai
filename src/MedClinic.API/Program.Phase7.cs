// Partial startup registration hook for Phase 7 services.
// Call this from Program.cs:
//   builder.Services.AddPhase7Services(builder.Configuration);
//
// Already wired when using AddInfrastructure() if that method
// calls AddPhase7Services internally.
//
// This file intentionally left as documentation anchor.
// Actual registration is in DependencyInjection.Phase7.cs
