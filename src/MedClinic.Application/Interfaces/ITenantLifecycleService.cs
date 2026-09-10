using MedClinic.Application.Features.SuperAdmin.DTOs;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;

namespace MedClinic.Application.Interfaces;

public interface ITenantLifecycleService
{
    Task<Clinic> ProvisionClinicAsync(ProvisionClinicRequest request, Guid? performedByUserId = null, string? ipAddress = null, CancellationToken ct = default);
    Task SuspendClinicAsync(Guid clinicId, string reason, Guid? performedByUserId = null, string? ipAddress = null, string? correlationId = null, CancellationToken ct = default);
    Task ReactivateClinicAsync(Guid clinicId, string? reason = null, Guid? performedByUserId = null, string? ipAddress = null, string? correlationId = null, CancellationToken ct = default);
    Task CancelClinicAsync(Guid clinicId, string reason, Guid? performedByUserId = null, string? ipAddress = null, string? correlationId = null, CancellationToken ct = default);
    Task ChangePlanAsync(Guid clinicId, Guid newPlanId, BillingCycle cycle, Guid? performedByUserId = null, string? ipAddress = null, CancellationToken ct = default);
    Task SetComplianceStatusAsync(Guid clinicId, TenantComplianceStatus status, string reason, Guid? performedByUserId = null, string? ipAddress = null, CancellationToken ct = default);
    Task<IReadOnlyList<TenantLifecycleAuditEventDto>> GetLifecycleAuditTrailAsync(Guid clinicId, CancellationToken ct = default);
}
