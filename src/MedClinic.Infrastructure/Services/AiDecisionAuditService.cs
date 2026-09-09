using System.Security.Cryptography;
using System.Text;
using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Features.AI.Services;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Services;

public class AiDecisionAuditService : IAiDecisionAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenant;
    private readonly ILogger<AiDecisionAuditService> _logger;

    public AiDecisionAuditService(
        IApplicationDbContext context,
        ITenantContext tenant,
        ILogger<AiDecisionAuditService> logger)
    {
        _context = context;
        _tenant = tenant;
        _logger = logger;
    }

    public async Task<AiDecisionAudit> RecordDecisionAsync(
        RecordAiDecisionRequest request,
        CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId
            ?? throw new UnauthorizedAccessException("Tenant clinic context is required to audit AI decisions.");

        var inputHash = ComputeSha256(request.InputPayload);
        var outputHash = ComputeSha256(request.OutputPayload);

        var audit = new AiDecisionAudit
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            VisitId = request.VisitId,
            Capability = request.Capability,
            ProviderName = request.ProviderName,
            ModelVersion = request.ModelVersion,
            InputHash = inputHash,
            OutputHash = outputHash,
            ConfidenceScore = request.ConfidenceScore,
            ReviewStatus = AiReviewStatus.PendingReview,
            CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        };

        _context.AiDecisionAudits.Add(audit);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Recorded AI decision audit {AuditId} for Capability '{Capability}', Provider '{Provider}', CorrelationId '{CorrelationId}'",
            audit.Id, audit.Capability, audit.ProviderName, audit.CorrelationId);

        return audit;
    }

    public async Task<AiDecisionAudit> ReviewDecisionAsync(
        Guid auditId,
        ReviewAiDecisionRequest request,
        Guid doctorUserId,
        CancellationToken ct = default)
    {
        var audit = await _context.AiDecisionAudits
            .FirstOrDefaultAsync(a => a.Id == auditId, ct);

        if (audit == null)
        {
            throw new KeyNotFoundException($"AI decision audit '{auditId}' not found.");
        }

        if (request.Status is AiReviewStatus.Rejected or AiReviewStatus.Modified)
        {
            if (string.IsNullOrWhiteSpace(request.OverrideReason))
            {
                throw new ArgumentException("A clinical override reason must be provided when rejecting or modifying an AI clinical recommendation.");
            }
        }

        audit.ReviewStatus = request.Status;
        audit.ReviewedByUserId = doctorUserId;
        audit.ReviewedAt = DateTime.UtcNow;
        audit.OverrideReason = request.OverrideReason;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "AI decision audit {AuditId} reviewed by Doctor {DoctorId}. New Status: {Status}",
            audit.Id, doctorUserId, audit.ReviewStatus);

        return audit;
    }

    public async Task<AiDecisionAuditDto?> GetAuditByIdAsync(
        Guid auditId,
        CancellationToken ct = default)
    {
        var audit = await _context.AiDecisionAudits
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.ReviewedByUser)
            .FirstOrDefaultAsync(a => a.Id == auditId, ct);

        return audit == null ? null : MapToDto(audit);
    }

    public async Task<PagedResult<AiDecisionAuditDto>> GetAuditsAsync(
        AiAuditFilterRequest filter,
        CancellationToken ct = default)
    {
        var query = _context.AiDecisionAudits
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.ReviewedByUser)
            .AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.ReviewStatus == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Capability))
        {
            query = query.Where(a => a.Capability == filter.Capability);
        }

        if (filter.PatientId.HasValue)
        {
            query = query.Where(a => a.PatientId == filter.PatientId.Value);
        }

        var total = await query.CountAsync(ct);
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<AiDecisionAuditDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static string ComputeSha256(string content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static AiDecisionAuditDto MapToDto(AiDecisionAudit audit) => new()
    {
        Id = audit.Id,
        ClinicId = audit.ClinicId,
        PatientId = audit.PatientId,
        PatientName = audit.Patient != null ? $"{audit.Patient.FirstName} {audit.Patient.LastName}".Trim() : null,
        DoctorId = audit.DoctorId,
        DoctorName = audit.Doctor != null ? $"{audit.Doctor.FirstName} {audit.Doctor.LastName}".Trim() : null,
        VisitId = audit.VisitId,
        Capability = audit.Capability,
        ProviderName = audit.ProviderName,
        ModelVersion = audit.ModelVersion,
        InputHash = audit.InputHash,
        OutputHash = audit.OutputHash,
        ConfidenceScore = audit.ConfidenceScore,
        ReviewStatus = audit.ReviewStatus,
        ReviewedByUserId = audit.ReviewedByUserId,
        ReviewedByUserName = audit.ReviewedByUser != null ? $"{audit.ReviewedByUser.FirstName} {audit.ReviewedByUser.LastName}".Trim() : null,
        ReviewedAt = audit.ReviewedAt,
        OverrideReason = audit.OverrideReason,
        CorrelationId = audit.CorrelationId,
        CreatedAt = audit.CreatedAt
    };
}
