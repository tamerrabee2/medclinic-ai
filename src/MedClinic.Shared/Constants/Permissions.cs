namespace MedClinic.Shared.Constants;

public static class Permissions
{
    // ── Patients ──────────────────────────────────────────────────────────
    public const string PatientsRead   = "Patients.Read";
    public const string PatientsCreate = "Patients.Create";
    public const string PatientsUpdate = "Patients.Update";
    public const string PatientsDelete = "Patients.Delete";

    // ── Appointments ─────────────────────────────────────────────────────
    public const string AppointmentsRead   = "Appointments.Read";
    public const string AppointmentsCreate = "Appointments.Create";
    public const string AppointmentsUpdate = "Appointments.Update";
    public const string AppointmentsCancel = "Appointments.Cancel";

    // ── Medical Records ───────────────────────────────────────────────────
    public const string MedicalRecordsRead   = "MedicalRecords.Read";
    public const string MedicalRecordsCreate = "MedicalRecords.Create";
    public const string MedicalRecordsUpdate = "MedicalRecords.Update";
    public const string MedicalRecordsDelete = "MedicalRecords.Delete";

    // ── Prescriptions ─────────────────────────────────────────────────────
    public const string PrescriptionsSign = "Prescriptions.Sign";

    // ── Lab ───────────────────────────────────────────────────────────────
    public const string LabRead   = "Lab.Read";
    public const string LabCreate = "Lab.Create";
    public const string LabUpdate = "Lab.Update";
    public const string LabEnterResults = "Lab.EnterResults";

    // ── Radiology ─────────────────────────────────────────────────────────
    public const string RadiologyRead   = "Radiology.Read";
    public const string RadiologyCreate = "Radiology.Create";
    public const string RadiologyUpdate = "Radiology.Update";
    public const string RadiologyReport = "Radiology.Report";
    public const string RadiologyAI     = "Radiology.AI";

    // ── Invoices / Billing ───────────────────────────────────────────────
    public const string BillingRead   = "Billing.Read";
    public const string BillingCreate = "Billing.Create";
    public const string BillingUpdate = "Billing.Update";
    public const string BillingDelete = "Billing.Delete";

    // ── Users / Staff ─────────────────────────────────────────────────────
    public const string UsersRead   = "Users.Read";
    public const string UsersManage = "Users.Manage";

    // ── Clinics ───────────────────────────────────────────────────────────
    public const string ClinicsRead   = "Clinics.Read";
    public const string ClinicsManage = "Clinics.Manage";

    // ── AI ────────────────────────────────────────────────────────────────
    public const string AIAssist = "AI.Assist";
    public const string AIAdmin  = "AI.Admin";

    // ── Reports ───────────────────────────────────────────────────────────
    public const string ReportsRead   = "Reports.Read";
    public const string ReportsExport = "Reports.Export";
}
