using MedClinic.Application.Common.Interfaces;
using MedClinic.Infrastructure.AI;
using MedClinic.Infrastructure.ExternalLabs;
using MedClinic.Infrastructure.FHIR;
using MedClinic.Infrastructure.Insurance;
using MedClinic.Infrastructure.PACS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedClinic.Infrastructure;

public static partial class DependencyInjection
{
    public static IServiceCollection AddPhase7Services(this IServiceCollection services, IConfiguration config)
    {
        // ── PACS Provider ──────────────────────────────────────────────────────
        var pacsProvider = config["PACS_PROVIDER"] ?? "Mock";
        if (pacsProvider.Equals("Orthanc", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IPacsProvider, OrthancPacsProvider>();
        }
        else
        {
            services.AddScoped<IPacsProvider, MockPacsProvider>();
        }

        // ── FHIR Provider ──────────────────────────────────────────────────────
        services.AddScoped<IFhirProvider, MockFhirProvider>();
        // TODO: swap with Hl7.Fhir.R4 client for production

        // ── External Labs Provider ─────────────────────────────────────────────
        services.AddScoped<IExternalLabProvider, MockExternalLabProvider>();

        // ── Insurance Gateway ──────────────────────────────────────────────────
        services.AddScoped<IInsuranceProviderGateway, MockInsuranceProviderGateway>();

        return services;
    }
}
