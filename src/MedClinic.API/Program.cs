using MedClinic.API.Middleware;
using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure.Extensions;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Infrastructure.Persistence.Seeder;
using MedClinic.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config  = builder.Configuration;

// ──────────────────────────────────────────────────────────────────
// DATABASE
// ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(
        config.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("MedClinic.Infrastructure")
    ));

// ──────────────────────────────────────────────────────────────────
// JWT AUTHENTICATION
// ──────────────────────────────────────────────────────────────────
var jwtKey    = config["Jwt:Key"]    ?? throw new InvalidOperationException("Jwt:Key missing.");
var jwtIssuer = config["Jwt:Issuer"] ?? "MedClinicAPI";
var jwtAudience = config["Jwt:Audience"] ?? "MedClinicClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

// ──────────────────────────────────────────────────────────────────
// AUTHORIZATION
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
// APPLICATION SERVICES
// ──────────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddInfrastructureServices();

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
        logger.LogError(ex, "Migration/Seeder error.");
        throw;
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
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
