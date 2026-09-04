namespace MedClinic.Application.Features.Canvas.DTOs;

// ── Medical Image Annotations ──────────────────────────────────────────────────

public record CreateAnnotationRequest(
    Guid   MedicalImageId,
    string Type,            // Pen | Arrow | Rectangle | Circle | Text | Measurement | Line
    string CoordinatesJson, // [{x,y}, ...]
    string? Color,
    int?   Thickness,
    string? Text,
    double? MeasurementValue,
    string? MeasurementUnit
);

public record UpdateAnnotationRequest(
    string? CoordinatesJson,
    string? Color,
    int?    Thickness,
    string? Text,
    double? MeasurementValue,
    string? MeasurementUnit
);

public record AnnotationDto(
    Guid    Id,
    Guid    MedicalImageId,
    Guid    DoctorId,
    string  DoctorName,
    string  Type,
    string  CoordinatesJson,
    string? Color,
    int     Thickness,
    string? Text,
    double? MeasurementValue,
    string? MeasurementUnit,
    bool    IsAIGenerated,
    double? AIConfidence,
    DateTime CreatedAt
);

public record SaveCanvasRequest(
    Guid           MedicalImageId,
    string         AnnotatedImageBase64, // base64 of the flattened annotated preview
    List<CreateAnnotationRequest> Annotations
);

// ── Body Map ──────────────────────────────────────────────────────────────────

public record CreateBodyAnnotationRequest(
    Guid    VisitId,
    Guid    PatientId,
    string  Region,
    string? Side,
    string? Symptom,
    int?    PainLevel,
    string? Notes,
    string? Diagnosis,
    double  PositionX,
    double  PositionY,
    string? MarkerColor
);

public record BodyAnnotationDto(
    Guid    Id,
    Guid    VisitId,
    string  Region,
    string? Side,
    string? Symptom,
    int?    PainLevel,
    string? Notes,
    string? Diagnosis,
    double  PositionX,
    double  PositionY,
    string  MarkerColor,
    DateTime CreatedAt
);

// ── Dental ───────────────────────────────────────────────────────────────────────

public record UpsertDentalRecordRequest(
    Guid    PatientId,
    Guid?   VisitId,
    int     ToothNumber,
    string  Condition,
    string? Surface,
    string? Notes,
    DateTime? TreatmentDate
);

public record DentalRecordDto(
    Guid    Id,
    int     ToothNumber,
    string  Condition,
    string? Surface,
    string? Notes,
    DateTime? TreatmentDate,
    string  DoctorName,
    DateTime CreatedAt
);

public record DentalChartDto(
    Guid   PatientId,
    string PatientName,
    List<DentalRecordDto> Teeth
);
