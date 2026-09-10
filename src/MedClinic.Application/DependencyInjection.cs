using MedClinic.Application.Features.AI.Services;
using MedClinic.Application.Features.Canvas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedClinic.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AIService>();
        services.AddScoped<CanvasService>();
        services.AddScoped<MedClinic.Application.Features.Patients.Services.PatientService>();
        services.AddScoped<MedClinic.Application.Features.Appointments.Services.AppointmentService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
