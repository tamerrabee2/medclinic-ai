using MedClinic.Application.Common.Interfaces;
using MedClinic.Infrastructure.ClinicalDecisionSupport;
using Microsoft.Extensions.DependencyInjection;

namespace MedClinic.Infrastructure;

public static partial class DependencyInjection
{
    /// <summary>Registers Phase 8 clinical decision support services.</summary>
    public static IServiceCollection AddPhase8Services(this IServiceCollection services)
    {
        services.AddScoped<IClinicalDecisionSupportService, RuleBasedClinicalDecisionSupportService>();
        return services;
    }
}
