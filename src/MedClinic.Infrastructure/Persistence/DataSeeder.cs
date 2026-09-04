using MedClinic.Domain.Entities;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roles = [
                Roles.SuperAdmin, Roles.ClinicAdmin, Roles.Doctor, Roles.Nurse,
                Roles.Receptionist, Roles.LabTechnician, Roles.Radiologist,
                Roles.Accountant, Roles.Patient
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            // Seed SuperAdmin
            const string superAdminEmail = "admin@medclinic.ai";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = superAdminEmail,
                    UserName = superAdminEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(superAdmin, "Admin@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
            }

            // Seed Demo Clinic
            if (!await context.Clinics.AnyAsync())
            {
                var clinic = new Clinic
                {
                    Name = "Demo Medical Center",
                    Slug = "demo-medical-center",
                    Email = "info@demo-clinic.com",
                    Phone = "+1-555-000-0000",
                    Address = "123 Healthcare Ave",
                    City = "Medical City",
                    Country = "US",
                    IsActive = true,
                    CreatedBy = superAdmin.Id
                };

                context.Clinics.Add(clinic);
                await context.SaveChangesAsync();

                context.ClinicMembers.Add(new ClinicMember
                {
                    ClinicId = clinic.Id,
                    UserId = superAdmin.Id,
                    Role = Roles.ClinicAdmin,
                    CreatedBy = superAdmin.Id
                });

                await context.SaveChangesAsync();
                logger.LogInformation("Demo clinic seeded: {ClinicName}", clinic.Name);
            }

            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
