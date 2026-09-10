namespace MedClinic.Shared.Constants;

public static class FeatureKey
{
    public const string AiCopilot       = "ai_copilot";
    public const string DicomPacs       = "dicom_pacs";
    public const string Dental          = "dental";
    public const string ExternalLabs    = "external_labs";
    public const string Insurance       = "insurance";
    public const string PatientPortal   = "patient_portal";
    public const string AdvancedReports = "advanced_reports";
    public const string CustomBranding  = "custom_branding";
    public const string ApiAccess       = "api_access";

    public static readonly IReadOnlyList<string> All =
    [
        AiCopilot,
        DicomPacs,
        Dental,
        ExternalLabs,
        Insurance,
        PatientPortal,
        AdvancedReports,
        CustomBranding,
        ApiAccess
    ];
}
