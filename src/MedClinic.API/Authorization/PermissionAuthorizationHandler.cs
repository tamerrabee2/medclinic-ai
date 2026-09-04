using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MedClinic.API.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check direct permission claim
        var hasPermission = context.User
            .Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // SuperAdmin gets all permissions
        var isSuperAdmin = context.User
            .Claims
            .Any(c => c.Type == ClaimTypes.Role && c.Value == MedClinic.Shared.Constants.Roles.SuperAdmin);

        if (isSuperAdmin)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
