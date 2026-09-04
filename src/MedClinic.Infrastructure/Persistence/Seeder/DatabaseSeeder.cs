using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BC = BCrypt.Net.BCrypt;

namespace MedClinic.Infrastructure.Persistence.Seeder;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting database seeder...");

        await SeedSuperAdminAsync(ct);
        await SeedDemoClinicAsync(ct);

        _logger.LogInformation("Database seeder completed.");
    }

    // ───────────────────────────────────────────────────────────────────

    private async Task SeedSuperAdminAsync(CancellationToken ct)
    {
        const string email = "superadmin@medclinic.ai";
        if (await _db.Users.AnyAsync(u => u.Email == email, ct)) return;

        var superAdmin = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Super",
            LastName     = "Admin",
            Email        = email,
            PasswordHash = BC.HashPassword("Admin@123!"),
            Role         = Roles.SuperAdmin,
            IsActive     = true,
            IsVerified   = true,
            CreatedAt    = DateTime.UtcNow
        };

        _db.Users.Add(superAdmin);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("SuperAdmin seeded: {Email}", email);
    }

    // ───────────────────────────────────────────────────────────────────

    private async Task SeedDemoClinicAsync(CancellationToken ct)
    {
        const string slug = "demo-clinic";
        if (await _db.Clinics.AnyAsync(c => c.Slug == slug, ct)) return;

        // ── Clinic ──
        var clinic = new Clinic
        {
            Id           = Guid.NewGuid(),
            Name         = "MedClinic Demo",
            Slug         = slug,
            Phone        = "+1-555-0100",
            Email        = "demo@medclinic.ai",
            Address      = "123 Health Street",
            City         = "New York",
            Country      = "US",
            Currency     = "USD",
            Timezone     = "America/New_York",
            InvoicePrefix = "INV",
            TaxRate      = 5m,
            DefaultAppointmentDuration = 30,
            AllowOnlineBooking = true,
            WorkingHoursStart  = new TimeOnly(8, 0),
            WorkingHoursEnd    = new TimeOnly(17, 0),
            WorkingDays        = "Mon,Tue,Wed,Thu,Fri",
            CreatedAt    = DateTime.UtcNow
        };
        _db.Clinics.Add(clinic);

        // ── Admin User ──
        var adminUser = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Clinic",
            LastName     = "Admin",
            Email        = "admin@medclinic.ai",
            PasswordHash = BC.HashPassword("Admin@123!"),
            Role         = Roles.ClinicAdmin,
            IsActive     = true,
            IsVerified   = true,
            CreatedAt    = DateTime.UtcNow
        };

        // ── Doctor User ──
        var doctorUser = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Ahmed",
            LastName     = "Hassan",
            Email        = "doctor@medclinic.ai",
            PasswordHash = BC.HashPassword("Doctor@123!"),
            Role         = Roles.Doctor,
            IsActive     = true,
            IsVerified   = true,
            CreatedAt    = DateTime.UtcNow
        };

        // ── Receptionist ──
        var receptionUser = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = "Sara",
            LastName     = "Ali",
            Email        = "reception@medclinic.ai",
            PasswordHash = BC.HashPassword("Staff@123!"),
            Role         = Roles.Receptionist,
            IsActive     = true,
            IsVerified   = true,
            CreatedAt    = DateTime.UtcNow
        };

        _db.Users.AddRange(adminUser, doctorUser, receptionUser);

        // ── Clinic Members ──
        _db.ClinicMembers.AddRange(
            new ClinicMember { ClinicId = clinic.Id, UserId = adminUser.Id,    Role = Roles.ClinicAdmin,  IsActive = true, JoinedAt = DateTime.UtcNow },
            new ClinicMember { ClinicId = clinic.Id, UserId = doctorUser.Id,   Role = Roles.Doctor,       IsActive = true, JoinedAt = DateTime.UtcNow },
            new ClinicMember { ClinicId = clinic.Id, UserId = receptionUser.Id, Role = Roles.Receptionist, IsActive = true, JoinedAt = DateTime.UtcNow }
        );

        // ── Doctor Profile ──
        var doctor = new Doctor
        {
            Id        = Guid.NewGuid(),
            ClinicId  = clinic.Id,
            UserId    = doctorUser.Id,
            Specialty = "General Practice",
            LicenseNumber = "GP-2024-001",
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Doctors.Add(doctor);

        // ── Sample Patients ──
        var patients = new[]
        {
            new Patient { Id = Guid.NewGuid(), ClinicId = clinic.Id, FirstName = "Mohammed", LastName = "Al-Rashid", DateOfBirth = new DateTime(1985, 3, 15), Gender = "Male",   Phone = "+1-555-1001", FileNumber = "P-001", CreatedAt = DateTime.UtcNow },
            new Patient { Id = Guid.NewGuid(), ClinicId = clinic.Id, FirstName = "Fatima",   LastName = "Nour",      DateOfBirth = new DateTime(1990, 7, 22), Gender = "Female", Phone = "+1-555-1002", FileNumber = "P-002", CreatedAt = DateTime.UtcNow },
            new Patient { Id = Guid.NewGuid(), ClinicId = clinic.Id, FirstName = "Khalid",   LastName = "Ibrahim",   DateOfBirth = new DateTime(1978, 11, 5), Gender = "Male",   Phone = "+1-555-1003", FileNumber = "P-003", CreatedAt = DateTime.UtcNow }
        };
        _db.Patients.AddRange(patients);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Demo clinic seeded: {Slug}", slug);
    }
}
