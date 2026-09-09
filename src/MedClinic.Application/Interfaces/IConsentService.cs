using MedClinic.Domain.Enums;

namespace MedClinic.Application.Interfaces;

public interface IConsentService
{
    Task<ConsentDto> RecordAsync(
        Guid patientId,
        ConsentType type,
        bool granted,
        DateTime? expiresAt,
        string? notes,
        Guid? witnessUserId,
        Guid? grantedByUserId,
        string? ipAddress,
        CancellationToken ct = default);

    Task<ConsentDto> RevokeAsync(
        Guid patientId,
        Guid consentId,
        string reason,
        Guid? revokedByUserId,
        string? ipAddress = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConsentDto>> GetHistoryAsync(
        Guid patientId,
        CancellationToken ct = default);

    Task<bool> HasActiveConsentAsync(
        Guid patientId,
        ConsentType type,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConsentAuditEventDto>> GetAuditTrailAsync(
        Guid patientId,
        Guid? consentId = null,
        CancellationToken ct = default);
}

public record ConsentDto(
    Guid Id,
    Guid PatientId,
    ConsentType ConsentType,
    bool IsGranted,
    DateTime GrantedAt,
    DateTime? ExpiresAt,
    Guid? WitnessUserId,
    Guid? GrantedByUserId,
    Guid? RevokedByUserId,
    DateTime? RevokedAt,
    string? RevocationReason,
    string? Notes,
    bool IsActive);

public record ConsentAuditEventDto(
    Guid Id,
    Guid ConsentRecordId,
    Guid PatientId,
    ConsentAuditEventType EventType,
    ConsentType ConsentType,
    Guid? PerformedByUserId,
    string? IpAddress,
    string? Reason,
    string? Details,
    DateTime Timestamp);