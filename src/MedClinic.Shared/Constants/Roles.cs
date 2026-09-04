namespace MedClinic.Shared.Constants;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ClinicAdmin = "ClinicAdmin";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";
    public const string LabTechnician = "LabTechnician";
    public const string Radiologist = "Radiologist";
    public const string Accountant = "Accountant";
    public const string Patient = "Patient";

    public static readonly IReadOnlyList<string> All =
    [
        SuperAdmin, ClinicAdmin, Doctor, Nurse, Receptionist,
        LabTechnician, Radiologist, Accountant, Patient
    ];

    public static readonly IReadOnlyList<string> ClinicalStaff =
    [
        ClinicAdmin, Doctor, Nurse, Receptionist,
        LabTechnician, Radiologist, Accountant
    ];
}
