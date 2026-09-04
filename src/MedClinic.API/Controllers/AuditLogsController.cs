using MedClinic.Application.Interfaces;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class AuditLogsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;

    public AuditLogsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ───────────────────────────────────────────────────────────────────
    // LIST
    // ───────────────────────────────────────────────────────────────────

    /// <summary>List audit logs with rich filters</summary>
    [HttpGet]
    [HasPermission(Permissions.ClinicsManage)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entityName,
        [FromQuery] string? action,
        [FromQuery] Guid?   userId,
        [FromQuery] Guid?   entityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 200);
        var clinicId = ClinicId;

        var query = _context.AuditLogs
            .Where(a => a.ClinicId == clinicId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);
        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from);
        if (to.HasValue)   query = query.Where(a => a.CreatedAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.EntityName,
                a.EntityId,
                a.Action,
                a.UserId,
                a.UserName,
                a.IpAddress,
                a.OldValues,
                a.NewValues,
                a.CreatedAt
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items      = items.Cast<object>().ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        });
    }

    // ───────────────────────────────────────────────────────────────────
    // ENTITY HISTORY
    // ───────────────────────────────────────────────────────────────────

    /// <summary>Get full audit history for a specific entity</summary>
    [HttpGet("entity/{entityName}/{entityId:guid}")]
    [HasPermission(Permissions.ClinicsManage)]
    public async Task<IActionResult> GetEntityHistory(
        string entityName,
        Guid   entityId,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var logs = await _context.AuditLogs
            .Where(a => a.ClinicId   == clinicId &&
                        a.EntityName == entityName &&
                        a.EntityId   == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id, a.Action, a.UserId, a.UserName,
                a.OldValues, a.NewValues, a.CreatedAt, a.IpAddress
            })
            .ToListAsync(ct);

        return Success(logs);
    }
}
