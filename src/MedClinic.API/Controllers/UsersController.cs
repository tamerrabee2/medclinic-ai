using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ITenantContext tenant)
    {
        _userManager = userManager;
        _context = context;
        _tenant = tenant;
    }

    /// <summary>List users in current clinic</summary>
    [HttpGet]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = _tenant.ClinicId
            ?? throw new UnauthorizedAccessException("Clinic context required.");

        var query = _context.ClinicMembers
            .Where(cm => cm.ClinicId == clinicId && !cm.IsDeleted)
            .Include(cm => cm.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(cm => cm.Role == role);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(cm =>
                cm.User.FirstName.Contains(search) ||
                cm.User.LastName.Contains(search) ||
                cm.User.Email!.Contains(search));

        var total = await query.CountAsync(ct);
        var members = await query
            .OrderBy(cm => cm.User.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cm => new
            {
                cm.User.Id,
                cm.User.FirstName,
                cm.User.LastName,
                FullName = cm.User.FirstName + " " + cm.User.LastName,
                cm.User.Email,
                cm.User.AvatarUrl,
                cm.User.IsActive,
                ClinicRole = cm.Role,
                cm.JoinedAt,
                cm.User.CreatedAt
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items = members.Cast<object>().ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>Get user profile by ID</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = _tenant.ClinicId
            ?? throw new UnauthorizedAccessException("Clinic context required.");

        var member = await _context.ClinicMembers
            .Where(cm => cm.ClinicId == clinicId && cm.UserId == id && !cm.IsDeleted)
            .Include(cm => cm.User)
            .Select(cm => new
            {
                cm.User.Id,
                cm.User.FirstName,
                cm.User.LastName,
                cm.User.Email,
                cm.User.PhoneNumber,
                cm.User.AvatarUrl,
                cm.User.IsActive,
                cm.User.PreferredLanguage,
                cm.User.CreatedAt,
                ClinicRole = cm.Role,
                cm.JoinedAt
            })
            .FirstOrDefaultAsync(ct);

        if (member == null) return NotFound("User not found in this clinic.");
        return Success(member);
    }

    /// <summary>Update own profile</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user == null) return NotFound("User not found.");

        if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage)) user.PreferredLanguage = request.PreferredLanguage;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

        return Success<object>(null!, "Profile updated.");
    }

    /// <summary>Activate or deactivate a user (ClinicAdmin only)</summary>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> SetStatus(
        Guid id,
        [FromBody] SetUserStatusRequest request,
        CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("User not found.");

        user.IsActive = request.IsActive;
        await _userManager.UpdateAsync(user);

        return Success<object>(null!, $"User {(request.IsActive ? "activated" : "deactivated")}.");
    }

    /// <summary>Assign system role to user (SuperAdmin only)</summary>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("User not found.");

        if (!Roles.All.Contains(request.Role))
            return BadRequest($"Invalid role: {request.Role}");

        if (await _userManager.IsInRoleAsync(user, request.Role))
            return BadRequest("User already has this role.");

        await _userManager.AddToRoleAsync(user, request.Role);
        return Success<object>(null!, $"Role '{request.Role}' assigned.");
    }

    /// <summary>Remove system role from user (SuperAdmin only)</summary>
    [HttpDelete("{id:guid}/roles/{role}")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> RemoveRole(Guid id, string role, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("User not found.");

        if (!await _userManager.IsInRoleAsync(user, role))
            return BadRequest("User does not have this role.");

        await _userManager.RemoveFromRoleAsync(user, role);
        return Success<object>(null!, $"Role '{role}' removed.");
    }
}

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? PreferredLanguage,
    string? AvatarUrl);

public record SetUserStatusRequest(bool IsActive);
public record AssignRoleRequest(string Role);
