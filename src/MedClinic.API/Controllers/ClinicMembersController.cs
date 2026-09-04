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
[Route("api/v1/clinics/{clinicId:guid}/members")]
public class ClinicMembersController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public ClinicMembersController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ITenantContext tenant)
    {
        _userManager = userManager;
        _context = context;
        _tenant = tenant;
    }

    /// <summary>List all members of a clinic</summary>
    [HttpGet]
    [HasPermission(Permissions.UsersRead)]
    public async Task<IActionResult> GetMembers(
        Guid clinicId,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);

        var query = _context.ClinicMembers
            .Where(cm => cm.ClinicId == clinicId && !cm.IsDeleted)
            .Include(cm => cm.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(cm => cm.Role == role);

        var total = await query.CountAsync(ct);
        var members = await query
            .OrderBy(cm => cm.User.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cm => new
            {
                cm.Id,
                cm.UserId,
                FullName = cm.User.FirstName + " " + cm.User.LastName,
                cm.User.Email,
                cm.User.AvatarUrl,
                cm.Role,
                cm.JoinedAt,
                cm.User.IsActive
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

    /// <summary>Invite existing user to clinic by email</summary>
    [HttpPost("invite")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> InviteMember(
        Guid clinicId,
        [FromBody] InviteMemberRequest request,
        CancellationToken ct)
    {
        // Validate role
        if (!Roles.ClinicalStaff.Contains(request.Role))
            return BadRequest($"Invalid clinic role: {request.Role}");

        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return NotFound($"No user found with email '{request.Email}'. They must register first.");

        // Check already a member
        var existing = await _context.ClinicMembers
            .FirstOrDefaultAsync(cm => cm.ClinicId == clinicId && cm.UserId == user.Id, ct);

        if (existing != null)
        {
            if (!existing.IsDeleted)
                return BadRequest("User is already a member of this clinic.");

            // Re-activate removed member
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.DeletedBy = null;
            existing.Role = request.Role;
            existing.JoinedAt = DateTime.UtcNow;
            existing.UpdatedBy = CurrentUserId;
            await _context.SaveChangesAsync(ct);
            return Success<object>(null!, "Member re-activated.");
        }

        var member = new ClinicMember
        {
            ClinicId = clinicId,
            UserId = user.Id,
            Role = request.Role,
            JoinedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId
        };

        _context.ClinicMembers.Add(member);

        // Assign system role if not already assigned
        if (!await _userManager.IsInRoleAsync(user, request.Role))
            await _userManager.AddToRoleAsync(user, request.Role);

        await _context.SaveChangesAsync(ct);

        return Created(new
        {
            MemberId = member.Id,
            UserId = user.Id,
            FullName = user.FullName,
            Role = request.Role
        }, "Member invited successfully.");
    }

    /// <summary>Update member's clinic role</summary>
    [HttpPatch("{memberId:guid}/role")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> UpdateRole(
        Guid clinicId,
        Guid memberId,
        [FromBody] UpdateMemberRoleRequest request,
        CancellationToken ct)
    {
        if (!Roles.ClinicalStaff.Contains(request.Role))
            return BadRequest($"Invalid clinic role: {request.Role}");

        var member = await _context.ClinicMembers
            .Include(cm => cm.User)
            .FirstOrDefaultAsync(cm =>
                cm.Id == memberId &&
                cm.ClinicId == clinicId &&
                !cm.IsDeleted, ct);

        if (member == null) return NotFound("Member not found.");

        // Prevent changing own role
        if (member.UserId == CurrentUserId)
            return BadRequest("You cannot change your own role.");

        var oldRole = member.Role;
        member.Role = request.Role;
        member.UpdatedBy = CurrentUserId;

        // Update Identity role
        if (await _userManager.IsInRoleAsync(member.User, oldRole))
            await _userManager.RemoveFromRoleAsync(member.User, oldRole);
        if (!await _userManager.IsInRoleAsync(member.User, request.Role))
            await _userManager.AddToRoleAsync(member.User, request.Role);

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, $"Role updated to '{request.Role}'.");
    }

    /// <summary>Remove member from clinic (soft delete)</summary>
    [HttpDelete("{memberId:guid}")]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> RemoveMember(
        Guid clinicId,
        Guid memberId,
        CancellationToken ct)
    {
        var member = await _context.ClinicMembers
            .FirstOrDefaultAsync(cm =>
                cm.Id == memberId &&
                cm.ClinicId == clinicId &&
                !cm.IsDeleted, ct);

        if (member == null) return NotFound("Member not found.");

        if (member.UserId == CurrentUserId)
            return BadRequest("You cannot remove yourself from the clinic.");

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
        member.DeletedBy = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Member removed from clinic.");
    }
}

public record InviteMemberRequest(string Email, string Role);
public record UpdateMemberRoleRequest(string Role);
