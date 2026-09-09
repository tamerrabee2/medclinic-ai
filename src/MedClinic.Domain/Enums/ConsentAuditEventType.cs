namespace MedClinic.Domain.Enums;

public enum ConsentAuditEventType
{
    Granted = 0,
    Revoked = 1,
    Expired = 2,
    Verified = 3,
    LegalHoldApplied = 4,
    LegalHoldReleased = 5
}
