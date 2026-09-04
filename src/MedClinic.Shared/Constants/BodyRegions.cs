namespace MedClinic.Shared.Constants;

/// <summary>
/// Standard body regions for the interactive body map.
/// These match the SVG region IDs on the frontend.
/// </summary>
public static class BodyRegions
{
    // Head & Neck
    public const string Head    = "Head";
    public const string Face    = "Face";
    public const string Neck    = "Neck";

    // Trunk — Anterior
    public const string Chest   = "Chest";
    public const string Abdomen = "Abdomen";
    public const string Pelvis  = "Pelvis";

    // Trunk — Posterior
    public const string UpperBack  = "UpperBack";
    public const string LowerBack  = "LowerBack";
    public const string Buttocks   = "Buttocks";

    // Upper Limbs
    public const string LeftShoulder  = "LeftShoulder";
    public const string RightShoulder = "RightShoulder";
    public const string LeftArm       = "LeftArm";
    public const string RightArm      = "RightArm";
    public const string LeftForearm   = "LeftForearm";
    public const string RightForearm  = "RightForearm";
    public const string LeftHand      = "LeftHand";
    public const string RightHand     = "RightHand";

    // Lower Limbs
    public const string LeftThigh  = "LeftThigh";
    public const string RightThigh = "RightThigh";
    public const string LeftKnee   = "LeftKnee";
    public const string RightKnee  = "RightKnee";
    public const string LeftLeg    = "LeftLeg";
    public const string RightLeg   = "RightLeg";
    public const string LeftFoot   = "LeftFoot";
    public const string RightFoot  = "RightFoot";

    public static readonly IReadOnlyList<string> All = [
        Head, Face, Neck,
        Chest, Abdomen, Pelvis,
        UpperBack, LowerBack, Buttocks,
        LeftShoulder, RightShoulder,
        LeftArm, RightArm,
        LeftForearm, RightForearm,
        LeftHand, RightHand,
        LeftThigh, RightThigh,
        LeftKnee, RightKnee,
        LeftLeg, RightLeg,
        LeftFoot, RightFoot
    ];
}
