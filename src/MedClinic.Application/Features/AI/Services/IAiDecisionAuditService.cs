using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Domain.Entities;
using MedClinic.Shared.Common;

namespace MedClinic.Application.Features.AI.Services;

public interface IAiDecisionAuditService
{
    Task<AiDecisionAudit> RecordDecisionAsync(
        RecordAiDecisionRequest request,
        CancellationToken ct = default);

    Task<AiDecisionAudit> ReviewDecisionAsync(
        Guid auditId,
        ReviewAiDecisionRequest request,
        Guid doctorUserId,
        CancellationToken ct = default);

    Task<AiDecisionAuditDto?> GetAuditByIdAsync(
        Guid auditId,
        CancellationToken ct = default);

    Task<PagedResult<AiDecisionAuditDto>> GetAuditsAsync(
        AiAuditFilterRequest filter,
        CancellationToken ct = default);
}
