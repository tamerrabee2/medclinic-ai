using System.Text;
using MedClinic.API.Authorization;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[ApiController]
[Route("api/v1/audit/explorer")]
[Authorize]
public class AuditExplorerController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public AuditExplorerController(ApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    private Guid ClinicId => _tenantContext.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    /// <summary>
    /// Search and filter patient consent lifecycle audit events across the clinic.
    /// </summary>
    [HttpGet("consents")]
    [HasPermission(Permissions.PatientConsentsAudit)]
    public async Task<IActionResult> GetConsentAuditEvents(
        [FromQuery] Guid? patientId,
        [FromQuery] ConsentAuditEventType? eventType,
        [FromQuery] ConsentType? consentType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        var clinicId = ClinicId;

        var query = _context.ConsentAuditEvents
            .Include(a => a.Patient)
            .Include(a => a.PerformedByUser)
            .Where(a => a.ClinicId == clinicId)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        if (eventType.HasValue)
            query = query.Where(a => a.EventType == eventType.Value);

        if (consentType.HasValue)
            query = query.Where(a => a.ConsentType == consentType.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.ConsentRecordId,
                a.PatientId,
                PatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "Unknown",
                PatientMrn = a.Patient != null ? (a.Patient.FileNumber ?? a.Patient.NationalId ?? string.Empty) : string.Empty,
                EventType = a.EventType.ToString(),
                ConsentType = a.ConsentType.ToString(),
                a.PerformedByUserId,
                PerformedByName = a.PerformedByUser != null ? a.PerformedByUser.FullName : "System",
                a.IpAddress,
                a.Reason,
                a.Details,
                a.Timestamp
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items = items.Cast<object>().ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Export consent audit events in CSV format for compliance and regulatory reporting.
    /// </summary>
    [HttpGet("consents/export")]
    [HasPermission(Permissions.PatientConsentsAudit)]
    public async Task<IActionResult> ExportConsentAuditCsv(
        [FromQuery] Guid? patientId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var clinicId = ClinicId;

        var query = _context.ConsentAuditEvents
            .Include(a => a.Patient)
            .Include(a => a.PerformedByUser)
            .Where(a => a.ClinicId == clinicId)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var records = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(5000)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("EventId,TimestampUtc,PatientMrn,PatientName,EventType,ConsentType,PerformedBy,Reason,IpAddress");

        foreach (var r in records)
        {
            var patientName = r.Patient != null ? $"{r.Patient.FirstName} {r.Patient.LastName}".Replace(",", " ") : "Unknown";
            var patientMrn = r.Patient?.FileNumber ?? r.Patient?.NationalId ?? "";
            var performedBy = r.PerformedByUser?.FullName.Replace(",", " ") ?? "System";
            var reason = (r.Reason ?? "").Replace(",", ";").Replace("\r\n", " ");

            sb.AppendLine($"{r.Id},{r.Timestamp:u},{patientMrn},{patientName},{r.EventType},{r.ConsentType},{performedBy},{reason},{r.IpAddress}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"consent_audit_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Apply or release Legal Hold on a patient consent or clinical record.
    /// Prevents archival or soft deletion under active litigation or compliance inquiry.
    /// </summary>
    [HttpPost("legal-hold")]
    [Authorize(Roles = $"{Roles.ClinicAdmin},{Roles.SuperAdmin}")]
    public async Task<IActionResult> SetLegalHold(
        [FromBody] SetLegalHoldRequest request,
        CancellationToken ct)
    {
        if (request.IsLegalHold && string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("A valid legal hold reason is mandatory when placing a hold.");
        }

        var clinicId = ClinicId;
        var record = await _context.ConsentRecords
            .FirstOrDefaultAsync(c => c.Id == request.ConsentRecordId && c.ClinicId == clinicId, ct);

        if (record == null)
            return NotFound("Consent record not found.");

        record.IsLegalHold = request.IsLegalHold;
        record.LegalHoldReason = request.IsLegalHold ? request.Reason : null;

        await _context.SaveChangesAsync(ct);

        return Success(new
        {
            consentId = record.Id,
            isLegalHold = record.IsLegalHold,
            legalHoldReason = record.LegalHoldReason,
            updatedAt = record.UpdatedAt
        }, request.IsLegalHold ? "Legal hold successfully placed." : "Legal hold successfully released.");
    }
}

public record SetLegalHoldRequest(
    Guid ConsentRecordId,
    bool IsLegalHold,
    string? Reason);
