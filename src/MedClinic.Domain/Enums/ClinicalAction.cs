namespace MedClinic.Domain.Enums;

public enum ClinicalAction
{
    // Preserved under Suspension (Read-only, Compliance, Legal)
    ViewRecords = 1,
    AccessAuditLogs = 2,
    RevokeConsent = 3,
    ManageLegalHold = 4,
    Login = 5,

    // Blocked under Suspension (Operations, Mutations, Consumption)
    CreateAppointment = 10,
    CreateVisit = 11,
    CreatePrescription = 12,
    OrderLab = 13,
    UploadDicom = 14,
    UseAiCopilot = 15,
    InviteUser = 16
}
