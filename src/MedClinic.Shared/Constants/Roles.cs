namespace MedClinic.Shared.Constants;

public static class Roles
{
    public const string SuperAdmin    = "SuperAdmin";
    public const string ClinicAdmin   = "ClinicAdmin";
    public const string Doctor        = "Doctor";
    public const string Nurse         = "Nurse";
    public const string Receptionist  = "Receptionist";
    public const string LabTechnician = "LabTechnician";
    public const string Radiologist   = "Radiologist";
    public const string Pharmacist    = "Pharmacist";
    public const string Accountant    = "Accountant";

    public static readonly IReadOnlyList<string> All =
    [
        SuperAdmin, ClinicAdmin, Doctor, Nurse,
        Receptionist, LabTechnician, Radiologist,
        Pharmacist, Accountant
    ];

    public static readonly IReadOnlyList<string> ClinicalRoles =
    [
        Doctor, Nurse, LabTechnician, Radiologist
    ];

    public static readonly IReadOnlyList<string> AdminRoles =
    [
        SuperAdmin, ClinicAdmin
    ];
}
