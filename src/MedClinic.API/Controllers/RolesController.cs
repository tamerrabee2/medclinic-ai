using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Identity;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize(Roles = Roles.SuperAdmin)]
public class RolesController : BaseController
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RolesController(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    /// <summary>List all system roles with their permissions</summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        var rolesWithPermissions = Roles.All.Select(role => new
        {
            Role = role,
            Permissions = PermissionClaimsFactory.GetPermissionsForRoles([role])
        });

        return Success(rolesWithPermissions);
    }

    /// <summary>Get permissions for a specific role</summary>
    [HttpGet("{role}/permissions")]
    public IActionResult GetPermissions(string role)
    {
        if (!Roles.All.Contains(role))
            return NotFound($"Role '{role}' not found.");

        var permissions = PermissionClaimsFactory.GetPermissionsForRoles([role]);
        return Success(new { Role = role, Permissions = permissions });
    }

    /// <summary>List users in a specific role</summary>
    [HttpGet("{role}/users")]
    public async Task<IActionResult> GetUsersInRole(
        string role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!Roles.All.Contains(role))
            return NotFound($"Role '{role}' not found.");

        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        var paged = usersInRole
            .OrderBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                FullName = u.FirstName + " " + u.LastName,
                u.Email,
                u.IsActive
            });

        return Success(new
        {
            Role = role,
            Total = usersInRole.Count,
            Page = page,
            PageSize = pageSize,
            Users = paged
        });
    }
}
