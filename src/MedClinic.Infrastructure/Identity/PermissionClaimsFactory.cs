using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MedClinic.Infrastructure.Identity;

/// <summary>
/// Adds permission claims to the JWT based on the user's role(s).
/// This maps Roles → Permissions so the JWT contains explicit permission claims.
/// </summary>
public class PermissionClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>
{
    private static readonly Dictionary<string, IReadOnlyList<string>> _rolePermissions = new()
    {
        [Roles.SuperAdmin] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate, Permissions.PatientsUpdate, Permissions.PatientsDelete,
            Permissions.MedicalRecordsRead, Permissions.MedicalRecordsCreate, Permissions.MedicalRecordsUpdate,
            Permissions.AppointmentsRead, Permissions.AppointmentsCreate, Permissions.AppointmentsUpdate, Permissions.AppointmentsCancel,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsCreate, Permissions.PrescriptionsUpdate,
            Permissions.LaboratoryRead, Permissions.LaboratoryCreate, Permissions.LaboratoryUpdate,
            Permissions.RadiologyRead, Permissions.RadiologyCreate, Permissions.RadiologyUpdate,
            Permissions.AIAnalysis, Permissions.AIReports, Permissions.AIApprove,
            Permissions.BillingRead, Permissions.BillingCreate, Permissions.BillingUpdate,
            Permissions.ClinicsManage, Permissions.ClinicsRead,
            Permissions.UsersManage, Permissions.UsersRead,
            Permissions.ReportsRead, Permissions.AuditLogsRead
        ],
        [Roles.ClinicAdmin] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate, Permissions.PatientsUpdate, Permissions.PatientsDelete,
            Permissions.MedicalRecordsRead, Permissions.MedicalRecordsCreate, Permissions.MedicalRecordsUpdate,
            Permissions.AppointmentsRead, Permissions.AppointmentsCreate, Permissions.AppointmentsUpdate, Permissions.AppointmentsCancel,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsCreate, Permissions.PrescriptionsUpdate,
            Permissions.LaboratoryRead, Permissions.LaboratoryCreate, Permissions.LaboratoryUpdate,
            Permissions.RadiologyRead, Permissions.RadiologyCreate, Permissions.RadiologyUpdate,
            Permissions.AIAnalysis, Permissions.AIReports, Permissions.AIApprove,
            Permissions.BillingRead, Permissions.BillingCreate, Permissions.BillingUpdate,
            Permissions.ClinicsRead, Permissions.UsersManage, Permissions.UsersRead, Permissions.ReportsRead
        ],
        [Roles.Doctor] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate, Permissions.PatientsUpdate,
            Permissions.MedicalRecordsRead, Permissions.MedicalRecordsCreate, Permissions.MedicalRecordsUpdate,
            Permissions.AppointmentsRead, Permissions.AppointmentsUpdate,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsCreate, Permissions.PrescriptionsUpdate,
            Permissions.LaboratoryRead, Permissions.LaboratoryCreate,
            Permissions.RadiologyRead, Permissions.RadiologyCreate,
            Permissions.AIAnalysis, Permissions.AIReports, Permissions.AIApprove,
            Permissions.BillingRead
        ],
        [Roles.Nurse] =
        [
            Permissions.PatientsRead, Permissions.PatientsUpdate,
            Permissions.MedicalRecordsRead, Permissions.MedicalRecordsCreate,
            Permissions.AppointmentsRead, Permissions.AppointmentsUpdate,
            Permissions.PrescriptionsRead,
            Permissions.LaboratoryRead,
            Permissions.RadiologyRead
        ],
        [Roles.Receptionist] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate, Permissions.PatientsUpdate,
            Permissions.AppointmentsRead, Permissions.AppointmentsCreate,
            Permissions.AppointmentsUpdate, Permissions.AppointmentsCancel,
            Permissions.BillingRead
        ],
        [Roles.LabTechnician] =
        [
            Permissions.PatientsRead,
            Permissions.LaboratoryRead, Permissions.LaboratoryCreate, Permissions.LaboratoryUpdate,
            Permissions.AIAnalysis
        ],
        [Roles.Radiologist] =
        [
            Permissions.PatientsRead,
            Permissions.RadiologyRead, Permissions.RadiologyCreate, Permissions.RadiologyUpdate,
            Permissions.AIAnalysis, Permissions.AIReports, Permissions.AIApprove
        ],
        [Roles.Accountant] =
        [
            Permissions.PatientsRead,
            Permissions.BillingRead, Permissions.BillingCreate, Permissions.BillingUpdate,
            Permissions.ReportsRead
        ],
        [Roles.Patient] =
        [
            Permissions.AppointmentsRead,
            Permissions.PrescriptionsRead,
            Permissions.LaboratoryRead,
            Permissions.RadiologyRead,
            Permissions.BillingRead
        ]
    };

    public PermissionClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles = await UserManager.GetRolesAsync(user);

        var permissions = new HashSet<string>();
        foreach (var role in roles)
        {
            if (_rolePermissions.TryGetValue(role, out var rolePerms))
                foreach (var perm in rolePerms)
                    permissions.Add(perm);
        }

        foreach (var permission in permissions)
            identity.AddClaim(new Claim("permission", permission));

        return identity;
    }

    /// <summary>Returns permissions for a given role (used for JWT generation).</summary>
    public static IReadOnlyList<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        var result = new HashSet<string>();
        foreach (var role in roles)
            if (_rolePermissions.TryGetValue(role, out var perms))
                foreach (var p in perms)
                    result.Add(p);
        return [.. result];
    }
}
