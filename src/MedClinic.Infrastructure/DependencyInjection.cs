using MedClinic.Application.Features.AI.Services;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.AI;
using MedClinic.Infrastructure.Audit;
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

namespace MedClinic.Infrastructure;

public static partial class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<MedClinic.Application.Common.Interfaces.IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"]
            ?? configuration["Jwt:Key"]
            ?? "SUPER_SECRET_FALLBACK_KEY_AT_LEAST_32_CHARS_LONG_123456";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "MedClinicAI",
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"] ?? "MedClinicAI",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Infrastructure Dependencies
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAiDecisionAuditService, AiDecisionAuditService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<MedClinic.Application.Common.Interfaces.IEmailService, EmailService>();
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<MedClinic.Application.Common.Interfaces.ICurrentUserService>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<MedClinic.Application.Common.Interfaces.IWhatsAppProvider, MedClinic.Infrastructure.Notifications.MockWhatsAppProvider>();
        services.AddScoped<MedClinic.Infrastructure.Notifications.NotificationService>();
        services.AddScoped<MedClinic.Application.Interfaces.INotificationService>(sp => sp.GetRequiredService<MedClinic.Infrastructure.Notifications.NotificationService>());
        services.AddScoped<MedClinic.Application.Common.Interfaces.INotificationService>(sp => sp.GetRequiredService<MedClinic.Infrastructure.Notifications.NotificationService>());
        services.AddScoped<MedClinic.Infrastructure.Billing.InvoiceStatusEngine>();
        services.AddScoped<MedClinic.Infrastructure.Persistence.Seeder.DatabaseSeeder>();

        // File Storage
        services.AddScoped<LocalFileStorage>();
        services.AddScoped<IFileStorage>(sp => sp.GetRequiredService<LocalFileStorage>());
        services.AddScoped<MedClinic.Application.Common.Interfaces.IFileStorageService>(sp => sp.GetRequiredService<LocalFileStorage>());

        // AI Provider
        var aiProvider = configuration["AI:Provider"] ?? "Mock";
        if (aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IAIProvider, OpenAIProvider>();
        else if (aiProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IAIProvider, GeminiProvider>();
        else if (aiProvider.Equals("Local", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IAIProvider, LocalAIProvider>();
        else
            services.AddScoped<IAIProvider, MockAIProvider>();

        // Caching
        services.AddDistributedMemoryCache();

        // Background Jobs
        services.AddHostedService<MedClinic.Infrastructure.BackgroundJobs.OverdueInvoiceJob>();
        services.AddHostedService<MedClinic.Infrastructure.BackgroundJobs.AppointmentReminderJob>();
        services.AddHostedService<MedClinic.Infrastructure.BackgroundJobs.DataCleanupJob>();

        return services;
    }
}
