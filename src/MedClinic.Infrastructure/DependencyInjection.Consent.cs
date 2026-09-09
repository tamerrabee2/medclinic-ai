using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
namespace MedClinic.Infrastructure;
public static partial class DependencyInjection { public static IServiceCollection AddConsentManagement(this IServiceCollection services){services.AddScoped<IConsentService,ConsentService>();return services;} }