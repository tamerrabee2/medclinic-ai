using MedClinic.Application.Common.Interfaces;
using MedClinic.Infrastructure.ClinicalDecisionSupport;
using Microsoft.Extensions.DependencyInjection;

namespace MedClinic.Infrastructure;

public static partial class DependencyInjection
{
    /// <summary>Registers the Phase 8.3–8.6 conservative clinical-AI baseline.</summary>
    public static IServiceCollection AddPhase8AdvancedClinicalAIServices(this IServiceCollection services)
    {
        services.AddScoped<IAdvancedClinicalAIService, ConservativeClinicalAIService>();
        return services;
    }
}
