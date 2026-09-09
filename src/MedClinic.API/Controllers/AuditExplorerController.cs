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
    /// Validates date range, pagination bounds, and tenant boundaries.
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
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return BadRequest(new { message = "'from' date cannot be after 'to' date." });
        }

        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);
        var clinicId = ClinicId;

        if (patientId.HasValue)
        {
            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == patientId.Value && p.ClinicId == clinicId, ct);
            if (!patientExists)
            {
                return NotFound(new { message = "Patient not found within clinic context." });
            }
        }

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
    /// Export consent audit events in CSV format with optional PII masking for compliance reporting.
    /// Unmasked PII is restricted to ClinicAdmin and SuperAdmin roles.
    /// Generates an immutable AuditLog record for every export operation.
    /// </summary>
    [HttpGet("consents/export")]
    [HasPermission(Permissions.PatientConsentsExport)]
    public async Task<IActionResult> ExportConsentAuditCsv(
        [FromQuery] Guid? patientId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool maskPii = true,
        CancellationToken ct = default)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return BadRequest(new { message = "'from' date cannot be after 'to' date." });
        }

        if (!maskPii && !User.IsInRole(Roles.ClinicAdmin) && !User.IsInRole(Roles.SuperAdmin))
        {
            return StatusCode(403, ApiResponse<object>.ErrorResult("Unmasked PII export is restricted to Compliance Officers and Clinic Administrators."));
        }

        var clinicId = ClinicId;

        if (patientId.HasValue)
        {
            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == patientId.Value && p.ClinicId == clinicId, ct);
            if (!patientExists)
            {
                return NotFound(new { message = "Patient not found within clinic context." });
            }
        }

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
        sb.AppendLine("EventId,TimestampUtc,PatientIdentifier,EventType,ConsentType,PerformedBy,Reason,IpAddress");

        foreach (var r in records)
        {
            var rawPatientName = r.Patient != null ? $"{r.Patient.FirstName} {r.Patient.LastName}" : "Unknown";
            var patientIdent = maskPii ? MaskName(rawPatientName) : rawPatientName;
            var performedBy = r.PerformedByUser?.FullName ?? "System";
            var reason = maskPii
                ? "[REDACTED — requires privileged export permission]"
                : (r.Reason ?? string.Empty);
            var ip = maskPii ? MaskIp(r.IpAddress) : (r.IpAddress ?? "N/A");

            sb.AppendLine($"{CsvSafe(r.Id.ToString())},{CsvSafe(r.Timestamp.ToString("u"))},{CsvSafe(patientIdent)},{CsvSafe(r.EventType.ToString())},{CsvSafe(r.ConsentType.ToString())},{CsvSafe(performedBy)},{CsvSafe(reason)},{CsvSafe(ip)}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);
        var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

        Response.Headers["X-Export-SHA256"] = hashString;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = CurrentUserId,
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? "User",
            EntityName = "ConsentAuditExport",
            EntityId = Guid.NewGuid(),
            Action = "ExportCsv",
            Description = $"Exported {records.Count} consent audit records (maskPii: {maskPii}, SHA256: {hashString}, PatientId: {patientId}, From: {from:u}, To: {to:u})",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        return File(bytes, "text/csv", $"consent_audit_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Search and filter clinical AI decision audits across the clinic.
    /// Standardized with permission-based authorization and default PII minimization.
    /// </summary>
    [HttpGet("ai-decisions")]
    [HasPermission(Permissions.AIDecisionsView)]
    public async Task<IActionResult> GetAiDecisionAudits(
        [FromQuery] Guid? patientId,
        [FromQuery] string? capability,
        [FromQuery] string? provider,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool maskPii = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return BadRequest(new { message = "'from' date cannot be after 'to' date." });
        }

        // Unmasked view is restricted to ClinicAdmin and SuperAdmin
        if (!maskPii && !User.IsInRole(Roles.ClinicAdmin) && !User.IsInRole(Roles.SuperAdmin))
        {
            maskPii = true;
        }

        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);
        var clinicId = ClinicId;

        var query = _context.AiDecisionAudits
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.ClinicId == clinicId)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        if (!string.IsNullOrWhiteSpace(capability))
            query = query.Where(a => a.Capability == capability);

        if (!string.IsNullOrWhiteSpace(provider))
            query = query.Where(a => a.ProviderName == provider);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var total = await query.CountAsync(ct);

        var rawItems = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.Capability,
                a.ProviderName,
                a.ModelVersion,
                a.PatientId,
                RawPatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "Unknown",
                RawFileNumber = a.Patient != null ? (a.Patient.FileNumber ?? a.Patient.NationalId ?? "") : "",
                a.DoctorId,
                DoctorName = a.Doctor != null ? a.Doctor.FullName : "System",
                ReviewStatus = a.ReviewStatus.ToString(),
                a.ConfidenceScore,
                a.OverrideReason,
                a.CorrelationId,
                a.CreatedAt,
                a.ReviewedAt
            })
            .ToListAsync(ct);

        var items = rawItems.Select(a => new
        {
            a.Id,
            a.Capability,
            a.ProviderName,
            a.ModelVersion,
            a.PatientId,
            PatientName = maskPii ? MaskName(a.RawPatientName) : a.RawPatientName,
            PatientFileNumber = maskPii ? MaskFileNumber(a.RawFileNumber) : a.RawFileNumber,
            a.DoctorId,
            a.DoctorName,
            a.ReviewStatus,
            a.ConfidenceScore,
            OverrideReason = maskPii && !string.IsNullOrEmpty(a.OverrideReason)
                ? "[REDACTED — requires privileged view permission]"
                : a.OverrideReason,
            a.CorrelationId,
            a.CreatedAt,
            a.ReviewedAt
        }).ToList();

        return Success(new PagedResult<object>
        {
            Items = items.Cast<object>().ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Export clinical AI decision audits in CSV format with formula injection sanitization and PII masking.
    /// Unmasked PII is restricted to ClinicAdmin and SuperAdmin roles.
    /// Generates an immutable AuditLog record for every export operation.
    /// </summary>
    [HttpGet("ai-decisions/export")]
    [HasPermission(Permissions.AIDecisionsExport)]
    public async Task<IActionResult> ExportAiDecisionAuditsCsv(
        [FromQuery] Guid? patientId,
        [FromQuery] string? capability,
        [FromQuery] string? provider,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool maskPii = true,
        CancellationToken ct = default)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return BadRequest(new { message = "'from' date cannot be after 'to' date." });
        }

        if (!maskPii && !User.IsInRole(Roles.ClinicAdmin) && !User.IsInRole(Roles.SuperAdmin))
        {
            return StatusCode(403, ApiResponse<object>.ErrorResult("Unmasked PII export is restricted to Compliance Officers and Clinic Administrators."));
        }

        var clinicId = ClinicId;

        var query = _context.AiDecisionAudits
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.ClinicId == clinicId)
            .AsQueryable();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        if (!string.IsNullOrWhiteSpace(capability))
            query = query.Where(a => a.Capability == capability);

        if (!string.IsNullOrWhiteSpace(provider))
            query = query.Where(a => a.ProviderName == provider);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var records = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(5000)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("DecisionId,TimestampUtc,PatientIdentifier,FileNumber,Capability,Provider,ModelVersion,Confidence,ReviewStatus,Doctor,OverrideReason");

        foreach (var r in records)
        {
            var rawPatientName = r.Patient != null ? $"{r.Patient.FirstName} {r.Patient.LastName}" : "Unknown";
            var rawFileNumber = r.Patient != null ? (r.Patient.FileNumber ?? r.Patient.NationalId ?? "") : "";
            var patientIdent = maskPii ? MaskName(rawPatientName) : rawPatientName;
            var fileNumber = maskPii ? MaskFileNumber(rawFileNumber) : rawFileNumber;
            var doctorName = r.Doctor?.FullName ?? "System";
            var overrideReason = maskPii
                ? "[REDACTED — requires privileged export permission]"
                : (r.OverrideReason ?? string.Empty);

            sb.AppendLine($"{CsvSafe(r.Id.ToString())},{CsvSafe(r.CreatedAt.ToString("u"))},{CsvSafe(patientIdent)},{CsvSafe(fileNumber)},{CsvSafe(r.Capability)},{CsvSafe(r.ProviderName)},{CsvSafe(r.ModelVersion)},{CsvSafe(r.ConfidenceScore?.ToString("P1") ?? "N/A")},{CsvSafe(r.ReviewStatus.ToString())},{CsvSafe(doctorName)},{CsvSafe(overrideReason)}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);
        var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();

        Response.Headers["X-Export-SHA256"] = hashString;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            UserId = CurrentUserId,
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? "User",
            EntityName = "AiDecisionAuditExport",
            EntityId = Guid.NewGuid(),
            Action = "ExportCsv",
            Description = $"Exported {records.Count} AI decision audit records (maskPii: {maskPii}, SHA256: {hashString}, PatientId: {patientId}, From: {from:u}, To: {to:u})",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        return File(bytes, "text/csv", $"ai_decision_audit_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Apply or release Legal Hold on a patient consent record.
    /// Automatically generates an immutable ConsentAuditEvent documenting the action.
    /// </summary>
    [HttpPost("legal-hold")]
    [Authorize(Roles = $"{Roles.ClinicAdmin},{Roles.SuperAdmin}")]
    public async Task<IActionResult> SetLegalHold(
        [FromBody] SetLegalHoldRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { message = "A valid legal hold reason is mandatory when placing or releasing a hold." });
        }

        var clinicId = ClinicId;
        var record = await _context.ConsentRecords
            .FirstOrDefaultAsync(c => c.Id == request.ConsentRecordId && c.ClinicId == clinicId, ct);

        if (record == null)
            return NotFound(new { message = "Consent record not found." });

        record.IsLegalHold = request.IsLegalHold;
        record.LegalHoldReason = request.IsLegalHold ? request.Reason : null;

        // Immutably audit the legal hold event
        var auditEvent = new ConsentAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            ConsentRecordId = record.Id,
            PatientId = record.PatientId,
            EventType = request.IsLegalHold
                ? ConsentAuditEventType.LegalHoldApplied
                : ConsentAuditEventType.LegalHoldReleased,
            ConsentType = record.ConsentType,
            PerformedByUserId = CurrentUserId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Reason = request.Reason,
            Details = request.IsLegalHold
                ? $"Active legal hold applied: {request.Reason}"
                : $"Active legal hold released: {request.Reason}",
            Timestamp = DateTime.UtcNow
        };
        _context.ConsentAuditEvents.Add(auditEvent);

        await _context.SaveChangesAsync(ct);

        return Success(new
        {
            consentId = record.Id,
            isLegalHold = record.IsLegalHold,
            legalHoldReason = record.LegalHoldReason,
            auditEventId = auditEvent.Id,
            updatedAt = record.UpdatedAt
        }, request.IsLegalHold ? "Legal hold successfully placed." : "Legal hold successfully released.");
    }

    private static string CsvSafe(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        var safe = value.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ");
        if (safe.StartsWith('=') || safe.StartsWith('+') || safe.StartsWith('-') || safe.StartsWith('@') || safe.StartsWith('\t'))
        {
            return $"\"'{safe}\"";
        }
        return $"\"{safe}\"";
    }

    private static string MaskName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts.Select(p => p.Length <= 1 ? p : $"{p[0]}***"));
    }

    private static string MaskFileNumber(string? fileNumber)
    {
        if (string.IsNullOrWhiteSpace(fileNumber)) return "N/A";
        if (fileNumber.Length <= 3) return "***";
        return $"{fileNumber[..2]}***{fileNumber[^1..]}";
    }

    private static string MaskIp(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return "0.0.0.0";
        var parts = ip.Split('.');
        if (parts.Length == 4) return $"{parts[0]}.{parts[1]}.***.***";
        return "***";
    }
}

public record SetLegalHoldRequest(
    Guid ConsentRecordId,
    bool IsLegalHold,
    string? Reason);
