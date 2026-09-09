using System.Threading.RateLimiting;
using MedClinic.API.Middleware;
using MedClinic.Application;
using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Persistence.Seeder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config  = builder.Configuration;

// Max Request Body Limit (25MB) for file uploads and payloads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 25 * 1024 * 1024; // 25 MB
});

// ──────────────────────────────────────────────────────────────────
// STARTUP ENVIRONMENT VALIDATION (P0)
// ──────────────────────────────────────────────────────────────────
if (builder.Environment.IsProduction())
{
    var jwtKey = config["Jwt:Key"] ?? config["Jwt:Secret"];
    if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || jwtKey.Contains("CHANGE_ME") || jwtKey.Contains("DEV_ONLY"))
    {
        throw new InvalidOperationException("CRITICAL: Production startup failed. JWT key is missing, weak (< 32 chars), or using an insecure default dev key.");
    }

    var aiProvider = config["AI:Provider"];
    var allowMock = config.GetValue<bool>("AllowMockInProduction");
    if (string.Equals(aiProvider, "Mock", StringComparison.OrdinalIgnoreCase) && !allowMock)
    {
        throw new InvalidOperationException("CRITICAL: Production startup failed. Mock AI provider cannot be used in Production without explicit AllowMockInProduction flag.");
    }
}

// ──────────────────────────────────────────────────────────────────
// INFRASTRUCTURE & APPLICATION DI
// ──────────────────────────────────────────────────────────────────
builder.Services.AddInfrastructure(config);
builder.Services.AddApplication();

// ──────────────────────────────────────────────────────────────────
// RATE LIMITING POLICIES (P0)
// ──────────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("ai-policy", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-Clinic-Id"].ToString() ?? "default-clinic",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 3,
                QueueLimit = 0
            }));

    options.AddPolicy("upload-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// ──────────────────────────────────────────────────────────────────
// AUTHORIZATION POLICIES
// ──────────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(opts =>
{
    // Dynamically register a policy for every permission constant
    var permissions = typeof(MedClinic.Shared.Constants.Permissions)
        .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .Select(f => (string)f.GetValue(null)!);

    foreach (var perm in permissions)
        opts.AddPolicy(perm, policy => policy.RequireClaim("permission", perm));
});

// ──────────────────────────────────────────────────────────────────
// CONTROLLERS + SWAGGER
// ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "MedClinic AI API",
        Version     = "v1",
        Description = "Multi-tenant clinic management system with AI assistant"
    });

    // JWT Auth in Swagger
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter JWT token"
    });
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod()));

// Health Checks
var healthCheckBuilder = builder.Services.AddHealthChecks();
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connStr = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connStr))
    {
        healthCheckBuilder.AddNpgSql(connStr);
    }
}

// ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate + Seed ──
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying migrations...");
        await db.Database.MigrateAsync();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not run database migration/seed on startup (database might be offline or starting up).");
    }
}

// ── Middleware Pipeline ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedClinic AI v1"));
}

// Security Headers Middleware (P1)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
