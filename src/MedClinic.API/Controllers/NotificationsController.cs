using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;

    public NotificationsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ───────────────────────────────────────────────────────────────────
    // MY NOTIFICATIONS
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Get current user’s notifications</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int   page     = 1,
        [FromQuery] int   pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var userId   = CurrentUserId;
        var clinicId = ClinicId;

        var query = _context.Notifications
            .Where(n => n.UserId == userId && n.ClinicId == clinicId)
            .AsQueryable();

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var total  = await query.CountAsync(ct);
        var unread = await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.ClinicId == clinicId && !n.IsRead, ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Body,
                n.Type,
                n.IsRead,
                n.ReadAt,
                n.EntityType,
                n.EntityId,
                n.CreatedAt
            })
            .ToListAsync(ct);

        return Success(new
        {
            TotalCount   = total,
            UnreadCount  = unread,
            Page         = page,
            PageSize     = pageSize,
            Items        = items
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // MARK READ
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Mark a single notification as read</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var n = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (n == null) return NotFound("Notification not found.");
        if (n.IsRead)  return Success<object>(null!, "Already read.");

        n.IsRead  = true;
        n.ReadAt  = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Marked as read.");
    }

    /// <summary>Mark ALL unread notifications as read</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId   = CurrentUserId;
        var clinicId = ClinicId;
        var now      = DateTime.UtcNow;

        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && n.ClinicId == clinicId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await _context.SaveChangesAsync(ct);
        return Success(new { Marked = unread.Count }, $"{unread.Count} notification(s) marked as read.");
    }

    // ───────────────────────────────────────────────────────────────────
    // DELETE
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Delete a notification</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var n = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (n == null) return NotFound("Notification not found.");

        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Notification deleted.");
    }

    // ───────────────────────────────────────────────────────────────────
    // UNREAD COUNT (for badge in UI)
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Get unread notification count only (lightweight for polling)</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var userId   = CurrentUserId;
        var clinicId = ClinicId;

        var count = await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.ClinicId == clinicId && !n.IsRead, ct);

        return Success(new { UnreadCount = count });
    }
}
