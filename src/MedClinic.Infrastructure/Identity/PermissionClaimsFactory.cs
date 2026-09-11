using MedClinic.Domain.Entities;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MedClinic.Infrastructure.Identity;

/// <summary>
/// Injects permission claims into the ClaimsPrincipal when user signs in.
/// Permissions are derived from Role — no DB lookup needed.
/// </summary>
public class PermissionClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>
{
    public PermissionClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles    = await UserManager.GetRolesAsync(user);

        foreach (var perm in GetPermissionsForRoles(roles))
            identity.AddClaim(new Claim("permission", perm));

        return identity;
    }

    public static IEnumerable<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        var perms = new HashSet<string>();
        foreach (var role in roles)
        {
            foreach (var p in GetOrEmpty(role))
                perms.Add(p);
        }
        return perms;
    }

    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        [Roles.SuperAdmin] =
        [
            // Full access — everything
            Permissions.PatientsRead,   Permissions.PatientsCreate,
            Permissions.PatientsUpdate, Permissions.PatientsDelete,
            Permissions.AppointmentsRead,   Permissions.AppointmentsCreate,
            Permissions.AppointmentsUpdate, Permissions.AppointmentsCancel,
            Permissions.MedicalRecordsRead,   Permissions.MedicalRecordsCreate,
            Permissions.MedicalRecordsUpdate, Permissions.MedicalRecordsDelete,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsCreate,
            Permissions.PrescriptionsUpdate, Permissions.PrescriptionsSign,
            Permissions.PrescriptionsVerify,
            Permissions.NotificationsRead, Permissions.NotificationsManage,
            Permissions.LabRead,   Permissions.LabCreate,
            Permissions.LabUpdate, Permissions.LabEnterResults,
            Permissions.RadiologyRead,   Permissions.RadiologyCreate,
            Permissions.RadiologyUpdate, Permissions.RadiologyReport,
            Permissions.RadiologyAI,
            Permissions.BillingRead,   Permissions.BillingCreate,
            Permissions.BillingUpdate, Permissions.BillingDelete,
            Permissions.UsersRead, Permissions.UsersManage,
            Permissions.ClinicsRead, Permissions.ClinicsManage,
            Permissions.AIAssist, Permissions.AIAdmin, Permissions.AIAnalysis,
            Permissions.AIDecisionsView, Permissions.AIDecisionsExport, Permissions.AIDecisionsReview,
            Permissions.ReportsRead, Permissions.ReportsExport,
            Permissions.PatientConsentsView, Permissions.PatientConsentsManage,
            Permissions.PatientConsentsRevoke, Permissions.PatientConsentsAudit,
            Permissions.PatientConsentsExport
        ],

        [Roles.ClinicAdmin] =
        [
            Permissions.PatientsRead,   Permissions.PatientsCreate,
            Permissions.PatientsUpdate, Permissions.PatientsDelete,
            Permissions.AppointmentsRead,   Permissions.AppointmentsCreate,
            Permissions.AppointmentsUpdate, Permissions.AppointmentsCancel,
            Permissions.MedicalRecordsRead,   Permissions.MedicalRecordsCreate,
            Permissions.MedicalRecordsUpdate,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsCreate,
            Permissions.PrescriptionsUpdate, Permissions.PrescriptionsSign,
            Permissions.PrescriptionsVerify,
            Permissions.NotificationsRead, Permissions.NotificationsManage,
            Permissions.LabRead,   Permissions.LabCreate,
            Permissions.LabUpdate, Permissions.LabEnterResults,
            Permissions.RadiologyRead,   Permissions.RadiologyCreate,
            Permissions.RadiologyUpdate, Permissions.RadiologyReport,
            Permissions.BillingRead,   Permissions.BillingCreate,
            Permissions.BillingUpdate,
            Permissions.UsersRead, Permissions.UsersManage,
            Permissions.ClinicsRead, Permissions.ClinicsManage,
            Permissions.AIAssist, Permissions.AIAnalysis,
            Permissions.AIDecisionsView, Permissions.AIDecisionsExport, Permissions.AIDecisionsReview,
            Permissions.ReportsRead, Permissions.ReportsExport,
            Permissions.PatientConsentsView, Permissions.PatientConsentsManage,
            Permissions.PatientConsentsRevoke, Permissions.PatientConsentsAudit,
            Permissions.PatientConsentsExport
        ],

        [Roles.Doctor] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate, Permissions.PatientsUpdate,
            Permissions.AppointmentsRead, Permissions.AppointmentsCreate, Permissions.AppointmentsUpdate,
            Permissions.MedicalRecordsRead, Permissions.MedicalRecordsCreate, Permissions.MedicalRecordsUpdate,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsCreate,
            Permissions.PrescriptionsUpdate, Permissions.PrescriptionsSign,
            Permissions.NotificationsRead,
            Permissions.LabRead, Permissions.LabCreate, Permissions.LabUpdate,
            Permissions.RadiologyRead, Permissions.RadiologyCreate, Permissions.RadiologyUpdate,
            Permissions.BillingRead,
            Permissions.AIAssist, Permissions.AIAnalysis,
            Permissions.AIDecisionsView, Permissions.AIDecisionsExport, Permissions.AIDecisionsReview,
            Permissions.ReportsRead,
            Permissions.PatientConsentsView, Permissions.PatientConsentsManage,
            Permissions.PatientConsentsRevoke, Permissions.PatientConsentsAudit,
            Permissions.PatientConsentsExport
        ],

        [Roles.Nurse] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate, Permissions.PatientsUpdate,
            Permissions.AppointmentsRead, Permissions.AppointmentsCreate, Permissions.AppointmentsUpdate,
            Permissions.MedicalRecordsRead, Permissions.MedicalRecordsCreate, Permissions.MedicalRecordsUpdate,
            Permissions.PrescriptionsRead,
            Permissions.NotificationsRead,
            Permissions.LabRead, Permissions.LabCreate,
            Permissions.RadiologyRead,
            Permissions.BillingRead,
            Permissions.PatientConsentsView, Permissions.PatientConsentsManage
        ],

        [Roles.Receptionist] =
        [
            Permissions.PatientsRead, Permissions.PatientsCreate,
            Permissions.AppointmentsRead, Permissions.AppointmentsCreate,
            Permissions.AppointmentsUpdate, Permissions.AppointmentsCancel,
            Permissions.BillingRead, Permissions.BillingCreate,
            Permissions.NotificationsRead,
            Permissions.PatientConsentsView
        ],

        [Roles.LabTechnician] =
        [
            Permissions.PatientsRead,
            Permissions.NotificationsRead,
            Permissions.LabRead, Permissions.LabCreate,
            Permissions.LabUpdate, Permissions.LabEnterResults
        ],

        [Roles.Radiologist] =
        [
            Permissions.PatientsRead,
            Permissions.NotificationsRead,
            Permissions.RadiologyRead,   Permissions.RadiologyCreate,
            Permissions.RadiologyUpdate, Permissions.RadiologyReport,
            Permissions.RadiologyAI,
            Permissions.AIAssist
        ],

        [Roles.Pharmacist] =
        [
            Permissions.PatientsRead,
            Permissions.MedicalRecordsRead,
            Permissions.PrescriptionsRead, Permissions.PrescriptionsVerify,
            Permissions.NotificationsRead,
            Permissions.BillingRead, Permissions.BillingCreate, Permissions.BillingUpdate
        ],

        [Roles.Accountant] =
        [
            Permissions.BillingRead, Permissions.BillingCreate,
            Permissions.BillingUpdate, Permissions.BillingDelete,
            Permissions.NotificationsRead,
            Permissions.ReportsRead, Permissions.ReportsExport
        ]
    };

    private static string[] GetOrEmpty(string role)
        => RolePermissions.TryGetValue(role, out var p) ? p : [];
}
