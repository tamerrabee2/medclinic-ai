using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.AI;
using MedClinic.Infrastructure.Identity;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Services;
using MedClinic.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MedClinic.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql
                    .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(3)
            ));

        services.AddScoped<IApplicationDbContext>(
            p => p.GetRequiredService<ApplicationDbContext>());

        // ── Identity ─────────────────────────────────────────────────
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit           = true;
                options.Password.RequireLowercase       = true;
                options.Password.RequireUppercase       = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength         = 8;
                options.User.RequireUniqueEmail         = true;
                options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail     = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<PermissionClaimsFactory>();   // ← injects permission claims

        // ── JWT Authentication ────────────────────────────────────────
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = configuration["Jwt:Issuer"] ?? "MedClinicAI",
                    ValidateAudience         = true,
                    ValidAudience            = configuration["Jwt:Audience"] ?? "MedClinicAI",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.FromSeconds(30)
                };

                // Support JWT from SignalR query string (for future real-time features)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            ctx.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        // ── Application Services ──────────────────────────────────────
        services.AddScoped<IJwtService,     JwtService>();
        services.AddScoped<IAuditService,   AuditService>();
        services.AddScoped<ITenantContext,  TenantContext>();
        services.AddHttpContextAccessor();

        // ── Storage ───────────────────────────────────────────────────
        var storageProvider = configuration["Storage:Provider"] ?? "Local";
        if (storageProvider.Equals("Local", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IFileStorage, LocalFileStorage>();
        // else: S3 / Azure Blob (Phase 6)

        // ── AI Provider ───────────────────────────────────────────────
        var aiProvider = configuration["AI:Provider"] ?? "Mock";
        if (aiProvider.Equals("Mock", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IAIProvider, MockAIProvider>();
        // else: OpenAI / Gemini (Phase 4)

        return services;
    }
}
