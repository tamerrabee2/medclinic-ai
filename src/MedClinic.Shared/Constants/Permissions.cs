namespace MedClinic.Shared.Constants;

public static class Permissions
{
    // Patients
    public const string PatientsRead = "Patients.Read";
    public const string PatientsCreate = "Patients.Create";
    public const string PatientsUpdate = "Patients.Update";
    public const string PatientsDelete = "Patients.Delete";

    // Medical Records
    public const string MedicalRecordsRead = "MedicalRecords.Read";
    public const string MedicalRecordsCreate = "MedicalRecords.Create";
    public const string MedicalRecordsUpdate = "MedicalRecords.Update";

    // Appointments
    public const string AppointmentsRead = "Appointments.Read";
    public const string AppointmentsCreate = "Appointments.Create";
    public const string AppointmentsUpdate = "Appointments.Update";
    public const string AppointmentsCancel = "Appointments.Cancel";

    // Prescriptions
    public const string PrescriptionsRead = "Prescriptions.Read";
    public const string PrescriptionsCreate = "Prescriptions.Create";
    public const string PrescriptionsUpdate = "Prescriptions.Update";

    // Laboratory
    public const string LaboratoryRead = "Laboratory.Read";
    public const string LaboratoryCreate = "Laboratory.Create";
    public const string LaboratoryUpdate = "Laboratory.Update";

    // Radiology
    public const string RadiologyRead = "Radiology.Read";
    public const string RadiologyCreate = "Radiology.Create";
    public const string RadiologyUpdate = "Radiology.Update";

    // AI
    public const string AIAnalysis = "AI.Analysis";
    public const string AIReports = "AI.Reports";
    public const string AIApprove = "AI.Approve";

    // Billing
    public const string BillingRead = "Billing.Read";
    public const string BillingCreate = "Billing.Create";
    public const string BillingUpdate = "Billing.Update";

    // Clinics
    public const string ClinicsManage = "Clinics.Manage";
    public const string ClinicsRead = "Clinics.Read";

    // Users
    public const string UsersManage = "Users.Manage";
    public const string UsersRead = "Users.Read";

    // Reports & Audit
    public const string ReportsRead = "Reports.Read";
    public const string AuditLogsRead = "AuditLogs.Read";
}
