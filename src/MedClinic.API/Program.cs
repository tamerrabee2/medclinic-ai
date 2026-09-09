using MedClinic.API.Middleware;
using MedClinic.Application;
using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Persistence.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config  = builder.Configuration;

// ──────────────────────────────────────────────────────────────────
// INFRASTRUCTURE & APPLICATION DI
// ──────────────────────────────────────────────────────────────────
builder.Services.AddInfrastructure(config);
builder.Services.AddApplication();

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

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
