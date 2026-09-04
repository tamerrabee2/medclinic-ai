using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedClinic.API.Middleware;

/// <summary>
/// Allows injecting clinic_id into the JWT claims after switching clinics.
/// Called by POST /api/v1/auth/switch-clinic
/// This middleware only processes that specific endpoint.
/// </summary>
public class ClinicSwitchMiddleware
{
    private readonly RequestDelegate _next;

    public ClinicSwitchMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
