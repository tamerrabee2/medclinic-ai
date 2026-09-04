using Microsoft.AspNetCore.Authorization;

namespace MedClinic.API.Authorization;

/// <summary>
/// Requires the authenticated user to have the specified permission claim.
/// Usage: [HasPermission(Permissions.PatientsRead)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(permission)
    {
        Policy = permission;
    }
}
