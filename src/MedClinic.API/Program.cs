using MedClinic.Infrastructure;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MedClinic.API")
    .WriteTo.Console()
    .WriteTo.File("logs/medclinic-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MedClinic AI API",
        Version = "v1",
        Description = "AI-powered Medical Clinic Management Platform API",
        Contact = new OpenApiContact { Name = "MedClinic AI", Email = "support@medclinic.ai" }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            }
        ] = []
    });

    c.EnableAnnotations();
});

// ── Infrastructure (DB, Identity, JWT, AI, Storage…) ─────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("MedClinicPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:3000"];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "sql", "postgresql"])
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter
        .Create<Microsoft.AspNetCore.Http.HttpContext, string>(ctx =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.User.Identity?.Name ?? ctx.Request.Headers.Host.ToString(),
                factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 200,
                    Window = TimeSpan.FromMinutes(1)
                }));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { success = false, message = "Too many requests. Please try again later." }, token);
    };
});

var app = builder.Build();

// ── Auto-migrate on startup (dev only) ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<MedClinic.API.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<MedClinic.API.Middleware.CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedClinic AI API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "MedClinic AI API";
    });
}

app.UseHttpsRedirection();
app.UseCors("MedClinicPolicy");
app.UseRateLimiter();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Health Check endpoints ────────────────────────────────────────────────────
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse,
    Predicate = _ => true
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.Run();

static async Task WriteHealthCheckResponse(
    Microsoft.AspNetCore.Http.HttpContext context,
    HealthReport report)
{
    context.Response.ContentType = "application/json";
    var result = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            duration = e.Value.Duration.TotalMilliseconds,
            description = e.Value.Description
        })
    };
    await context.Response.WriteAsJsonAsync(result);
}
