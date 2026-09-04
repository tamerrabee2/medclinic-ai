namespace MedClinic.Shared.Constants;

/// <summary>
/// Standard dental conditions using FDI tooth notation.
/// </summary>
public static class DentalConditions
{
    public const string Healthy    = "Healthy";
    public const string Cavity     = "Cavity";
    public const string Filling    = "Filling";
    public const string Crown      = "Crown";
    public const string Missing    = "Missing";
    public const string Implant    = "Implant";
    public const string RootCanal  = "RootCanal";
    public const string Extraction = "Extraction";
    public const string Fracture   = "Fracture";
    public const string Veneer     = "Veneer";
    public const string Bridge     = "Bridge";

    public static readonly IReadOnlyList<string> All = [
        Healthy, Cavity, Filling, Crown, Missing,
        Implant, RootCanal, Extraction, Fracture, Veneer, Bridge
    ];

    /// <summary>Color coding for the interactive dental chart UI</summary>
    public static readonly IReadOnlyDictionary<string, string> Colors =
        new Dictionary<string, string>
        {
            [Healthy]    = "#22C55E",
            [Cavity]     = "#EF4444",
            [Filling]    = "#3B82F6",
            [Crown]      = "#F59E0B",
            [Missing]    = "#6B7280",
            [Implant]    = "#8B5CF6",
            [RootCanal]  = "#F97316",
            [Extraction] = "#EC4899",
            [Fracture]   = "#DC2626",
            [Veneer]     = "#06B6D4",
            [Bridge]     = "#84CC16"
        };
}
