using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Services;

public sealed class ConsentService : IConsentService
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantContext _tenant;

    public ConsentService(ApplicationDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    private Guid CurrentClinicId => _tenant.ClinicId ?? throw new UnauthorizedAccessException("Tenant clinic context is required.");

    public async Task<ConsentDto> RecordAsync(
        Guid patientId,
        ConsentType type,
        bool granted,
        DateTime? expiresAt,
        string? notes,
        Guid? witnessUserId,
        Guid? grantedByUserId,
        string? ipAddress,
        CancellationToken ct = default)
    {
        var clinicId = CurrentClinicId;

        var patient = await _db.Patients
            .FirstOrDefaultAsync(x => x.Id == patientId && x.ClinicId == clinicId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (expiresAt is not null && expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Consent expiry must be in the future.");

        var consent = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            PatientId = patientId,
            ConsentType = type,
            IsGranted = granted,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            WitnessUserId = witnessUserId,
            GrantedByUserId = grantedByUserId,
            IpAddress = ipAddress,
            Notes = notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.ConsentRecords.Add(consent);

        var auditEvent = new ConsentAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            ConsentRecordId = consent.Id,
            PatientId = patientId,
            EventType = ConsentAuditEventType.Granted,
            ConsentType = type,
            PerformedByUserId = grantedByUserId,
            IpAddress = ipAddress,
            Details = $"Consent granted for {type}. Witness: {witnessUserId?.ToString() ?? "None"}.",
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.ConsentAuditEvents.Add(auditEvent);

        await _db.SaveChangesAsync(ct);

        return Map(consent);
    }

    public async Task<ConsentDto> RevokeAsync(
        Guid patientId,
        Guid consentId,
        string reason,
        Guid? revokedByUserId,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var clinicId = CurrentClinicId;

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revocation reason is required.");

        var consent = await _db.ConsentRecords
            .FirstOrDefaultAsync(x => x.Id == consentId && x.PatientId == patientId && x.ClinicId == clinicId, ct)
            ?? throw new KeyNotFoundException("Consent record not found.");

        // Guard against mutating an already-revoked record
        if (!consent.IsGranted || consent.RevokedAt != null)
            throw new InvalidOperationException("A revoked consent record cannot be modified or re-revoked.");

        var trimmedReason = reason.Trim();
        var now = DateTime.UtcNow;

        consent.IsGranted = false;
        consent.ExpiresAt = now;
        consent.RevokedAt = now;
        consent.RevokedByUserId = revokedByUserId;
        consent.RevocationReason = trimmedReason;
        consent.Notes = $"Revoked: {trimmedReason}";
        consent.UpdatedAt = now;

        var auditEvent = new ConsentAuditEvent
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            ConsentRecordId = consent.Id,
            PatientId = patientId,
            EventType = ConsentAuditEventType.Revoked,
            ConsentType = consent.ConsentType,
            PerformedByUserId = revokedByUserId,
            IpAddress = ipAddress,
            Reason = trimmedReason,
            Details = $"Consent revoked for {consent.ConsentType}. Reason: {trimmedReason}",
            Timestamp = now,
            CreatedAt = now
        };

        _db.ConsentAuditEvents.Add(auditEvent);

        await _db.SaveChangesAsync(ct);

        return Map(consent);
    }

    public async Task<IReadOnlyList<ConsentDto>> GetHistoryAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        var clinicId = CurrentClinicId;

        var patientExists = await _db.Patients
            .AnyAsync(x => x.Id == patientId && x.ClinicId == clinicId, ct);
        if (!patientExists)
            throw new KeyNotFoundException("Patient not found.");

        return await _db.ConsentRecords
            .Where(x => x.PatientId == patientId && x.ClinicId == clinicId)
            .OrderByDescending(x => x.GrantedAt)
            .Select(x => new ConsentDto(
                x.Id,
                x.PatientId,
                x.ConsentType,
                x.IsGranted,
                x.GrantedAt,
                x.ExpiresAt,
                x.WitnessUserId,
                x.GrantedByUserId,
                x.RevokedByUserId,
                x.RevokedAt,
                x.RevocationReason,
                x.Notes,
                x.IsGranted && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow)))
            .ToListAsync(ct);
    }

    public async Task<bool> HasActiveConsentAsync(
        Guid patientId,
        ConsentType type,
        CancellationToken ct = default)
    {
        var clinicId = CurrentClinicId;

        var patientExists = await _db.Patients
            .AnyAsync(x => x.Id == patientId && x.ClinicId == clinicId, ct);
        if (!patientExists)
            throw new KeyNotFoundException("Patient not found.");

        return await _db.ConsentRecords
            .AnyAsync(x => x.PatientId == patientId
                        && x.ClinicId == clinicId
                        && x.ConsentType == type
                        && x.IsGranted
                        && x.RevokedAt == null
                        && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow), ct);
    }

    public async Task<IReadOnlyList<ConsentAuditEventDto>> GetAuditTrailAsync(
        Guid patientId,
        Guid? consentId = null,
        CancellationToken ct = default)
    {
        var clinicId = CurrentClinicId;

        var patientExists = await _db.Patients
            .AnyAsync(x => x.Id == patientId && x.ClinicId == clinicId, ct);
        if (!patientExists)
            throw new KeyNotFoundException("Patient not found.");

        var query = _db.ConsentAuditEvents
            .Where(x => x.PatientId == patientId && x.ClinicId == clinicId);

        if (consentId.HasValue)
            query = query.Where(x => x.ConsentRecordId == consentId.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new ConsentAuditEventDto(
                x.Id,
                x.ConsentRecordId,
                x.PatientId,
                x.EventType,
                x.ConsentType,
                x.PerformedByUserId,
                x.IpAddress,
                x.Reason,
                x.Details,
                x.Timestamp))
            .ToListAsync(ct);
    }

    private static ConsentDto Map(ConsentRecord x) =>
        new(
            x.Id,
            x.PatientId,
            x.ConsentType,
            x.IsGranted,
            x.GrantedAt,
            x.ExpiresAt,
            x.WitnessUserId,
            x.GrantedByUserId,
            x.RevokedByUserId,
            x.RevokedAt,
            x.RevocationReason,
            x.Notes,
            x.IsGranted && x.RevokedAt == null && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow));
}