using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Persistence.Seeder;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public DatabaseSeeder(ApplicationDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting database seeder...");

        await SeedRolesAsync(ct);
        await SeedSuperAdminAsync(ct);
        await SeedDemoClinicAsync(ct);

        _logger.LogInformation("Database seeder completed.");
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        foreach (var roleName in Roles.All)
        {
            if (!await _db.Roles.AnyAsync(r => r.Name == roleName, ct))
            {
                _db.Roles.Add(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                });
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedSuperAdminAsync(CancellationToken ct)
    {
        const string email = "superadmin@medclinic.ai";
        var superAdmin = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                Id                 = Guid.NewGuid(),
                FirstName          = "Super",
                LastName           = "Admin",
                Email              = email,
                UserName           = email,
                NormalizedEmail    = email.ToUpperInvariant(),
                NormalizedUserName = email.ToUpperInvariant(),
                EmailConfirmed     = true,
                IsActive           = true,
                CreatedAt          = DateTime.UtcNow,
                SecurityStamp      = Guid.NewGuid().ToString()
            };
            superAdmin.PasswordHash = _hasher.HashPassword(superAdmin, "Admin@123!");

            _db.Users.Add(superAdmin);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("SuperAdmin seeded: {Email}", email);
        }

        var superAdminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == Roles.SuperAdmin, ct);
        if (superAdminRole != null)
        {
            var hasRole = await _db.UserRoles.AnyAsync(ur => ur.UserId == superAdmin.Id && ur.RoleId == superAdminRole.Id, ct);
            if (!hasRole)
            {
                _db.UserRoles.Add(new IdentityUserRole<Guid>
                {
                    UserId = superAdmin.Id,
                    RoleId = superAdminRole.Id
                });
                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("SuperAdmin assigned to role {Role}", Roles.SuperAdmin);
            }
        }
    }

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
        var adminUser = new ApplicationUser
        {
            Id                 = Guid.NewGuid(),
            FirstName          = "Clinic",
            LastName           = "Admin",
            Email              = "admin@medclinic.ai",
            UserName           = "admin@medclinic.ai",
            NormalizedEmail    = "ADMIN@MEDCLINIC.AI",
            NormalizedUserName = "ADMIN@MEDCLINIC.AI",
            EmailConfirmed     = true,
            IsActive           = true,
            CreatedAt          = DateTime.UtcNow,
            SecurityStamp      = Guid.NewGuid().ToString()
        };
        adminUser.PasswordHash = _hasher.HashPassword(adminUser, "Admin@123!");

        // ── Doctor User ──
        var doctorUser = new ApplicationUser
        {
            Id                 = Guid.NewGuid(),
            FirstName          = "Ahmed",
            LastName           = "Hassan",
            Email              = "doctor@medclinic.ai",
            UserName           = "doctor@medclinic.ai",
            NormalizedEmail    = "DOCTOR@MEDCLINIC.AI",
            NormalizedUserName = "DOCTOR@MEDCLINIC.AI",
            EmailConfirmed     = true,
            IsActive           = true,
            CreatedAt          = DateTime.UtcNow,
            SecurityStamp      = Guid.NewGuid().ToString()
        };
        doctorUser.PasswordHash = _hasher.HashPassword(doctorUser, "Doctor@123!");

        // ── Receptionist ──
        var receptionUser = new ApplicationUser
        {
            Id                 = Guid.NewGuid(),
            FirstName          = "Sara",
            LastName           = "Ali",
            Email              = "reception@medclinic.ai",
            UserName           = "reception@medclinic.ai",
            NormalizedEmail    = "RECEPTION@MEDCLINIC.AI",
            NormalizedUserName = "RECEPTION@MEDCLINIC.AI",
            EmailConfirmed     = true,
            IsActive           = true,
            CreatedAt          = DateTime.UtcNow,
            SecurityStamp      = Guid.NewGuid().ToString()
        };
        receptionUser.PasswordHash = _hasher.HashPassword(receptionUser, "Staff@123!");

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
            new Patient { Id = Guid.NewGuid(), ClinicId = clinic.Id, FirstName = "Mohammed", LastName = "Al-Rashid", DateOfBirth = new DateTime(1985, 3, 15), Gender = Gender.Male,   Phone = "+1-555-1001", NationalId = "P-001", CreatedAt = DateTime.UtcNow },
            new Patient { Id = Guid.NewGuid(), ClinicId = clinic.Id, FirstName = "Fatima",   LastName = "Nour",      DateOfBirth = new DateTime(1990, 7, 22), Gender = Gender.Female, Phone = "+1-555-1002", NationalId = "P-002", CreatedAt = DateTime.UtcNow },
            new Patient { Id = Guid.NewGuid(), ClinicId = clinic.Id, FirstName = "Khalid",   LastName = "Ibrahim",   DateOfBirth = new DateTime(1978, 11, 5), Gender = Gender.Male,   Phone = "+1-555-1003", NationalId = "P-003", CreatedAt = DateTime.UtcNow }
        };
        _db.Patients.AddRange(patients);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Demo clinic seeded: {Slug}", slug);
    }
}
