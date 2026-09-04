namespace MedClinic.Shared.Constants;

/// <summary>
/// Canvas annotation tool types — must match frontend tool identifiers.
/// </summary>
public static class AnnotationTypes
{
    public const string Pen         = "Pen";
    public const string Brush       = "Brush";
    public const string Arrow       = "Arrow";
    public const string Line        = "Line";
    public const string Rectangle   = "Rectangle";
    public const string Circle      = "Circle";
    public const string Text        = "Text";
    public const string Measurement = "Measurement";
    public const string Eraser      = "Eraser";
    public const string Region      = "Region";       // Freehand region highlight

    public static readonly IReadOnlyList<string> All = [
        Pen, Brush, Arrow, Line, Rectangle, Circle, Text, Measurement, Region
    ];
}
