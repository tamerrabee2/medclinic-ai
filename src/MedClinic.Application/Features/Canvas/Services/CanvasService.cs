using MedClinic.Application.Features.Canvas.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Canvas.Services;

public class CanvasService
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantContext       _tenant;
    private readonly IFileStorage         _storage;

    public CanvasService(
        ApplicationDbContext db,
        ITenantContext tenant,
        IFileStorage storage)
    {
        _db      = db;
        _tenant  = tenant;
        _storage = storage;
    }

    // ───────────────────────────────────────────────────────────────────
    // Image Annotations
    // ───────────────────────────────────────────────────────────────────

    public async Task<List<AnnotationDto>> GetAnnotationsAsync(
        Guid imageId, CancellationToken ct = default)
    {
        return await _db.MedicalAnnotations
            .Where(a => a.MedicalImageId == imageId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AnnotationDto(
                a.Id, a.MedicalImageId, a.DoctorId,
                a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : "Unknown",
                a.Type, a.CoordinatesJson, a.Color,
                a.Thickness, a.Text, a.MeasurementValue, a.MeasurementUnit,
                a.IsAIGenerated, a.AIConfidence, a.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<AnnotationDto> AddAnnotationAsync(
        Guid doctorId, CreateAnnotationRequest req, CancellationToken ct = default)
    {
        var annotation = new MedicalAnnotation
        {
            Id             = Guid.NewGuid(),
            MedicalImageId = req.MedicalImageId,
            DoctorId       = doctorId,
            Type           = req.Type,
            CoordinatesJson = req.CoordinatesJson,
            Color          = req.Color ?? "#FF0000",
            Thickness      = req.Thickness ?? 2,
            Text           = req.Text,
            MeasurementValue = req.MeasurementValue,
            MeasurementUnit  = req.MeasurementUnit,
            CreatedAt      = DateTime.UtcNow
        };

        _db.MedicalAnnotations.Add(annotation);
        await _db.SaveChangesAsync(ct);

        return new AnnotationDto(
            annotation.Id, annotation.MedicalImageId, annotation.DoctorId,
            "Doctor", annotation.Type, annotation.CoordinatesJson,
            annotation.Color, annotation.Thickness, annotation.Text,
            annotation.MeasurementValue, annotation.MeasurementUnit,
            false, null, annotation.CreatedAt);
    }

    public async Task DeleteAnnotationAsync(
        Guid annotationId, Guid doctorId, CancellationToken ct = default)
    {
        var annotation = await _db.MedicalAnnotations
            .FirstOrDefaultAsync(a => a.Id == annotationId && a.DoctorId == doctorId, ct)
            ?? throw new KeyNotFoundException("Annotation not found.");

        _db.MedicalAnnotations.Remove(annotation);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Save entire canvas: bulk annotations + annotated preview image.
    /// Original image is never modified.
    /// </summary>
    public async Task SaveCanvasAsync(
        Guid doctorId, SaveCanvasRequest req, CancellationToken ct = default)
    {
        // 1. Remove previous annotations for this image by this doctor
        var existing = await _db.MedicalAnnotations
            .Where(a => a.MedicalImageId == req.MedicalImageId && a.DoctorId == doctorId)
            .ToListAsync(ct);
        _db.MedicalAnnotations.RemoveRange(existing);

        // 2. Save new annotations
        var annotations = req.Annotations.Select(r => new MedicalAnnotation
        {
            Id              = Guid.NewGuid(),
            MedicalImageId  = req.MedicalImageId,
            DoctorId        = doctorId,
            Type            = r.Type,
            CoordinatesJson = r.CoordinatesJson,
            Color           = r.Color ?? "#FF0000",
            Thickness       = r.Thickness ?? 2,
            Text            = r.Text,
            MeasurementValue = r.MeasurementValue,
            MeasurementUnit  = r.MeasurementUnit,
            CreatedAt       = DateTime.UtcNow
        }).ToList();

        _db.MedicalAnnotations.AddRange(annotations);

        // 3. Save annotated preview
        if (!string.IsNullOrWhiteSpace(req.AnnotatedImageBase64))
        {
            var bytes  = Convert.FromBase64String(req.AnnotatedImageBase64);
            var stream = new MemoryStream(bytes);
            var path   = $"annotations/{req.MedicalImageId}/preview_{doctorId}.jpg";
            await _storage.UploadAsync(path, stream, "image/jpeg", ct);

            // Update the RadiologyImage annotated path
            var image = await _db.RadiologyImages
                .FirstOrDefaultAsync(i => i.Id == req.MedicalImageId, ct);
            if (image != null)
            {
                image.AnnotatedImagePath = path;
                image.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    // ───────────────────────────────────────────────────────────────────
    // Body Map
    // ───────────────────────────────────────────────────────────────────

    public async Task<List<BodyAnnotationDto>> GetBodyAnnotationsAsync(
        Guid visitId, CancellationToken ct = default)
    {
        return await _db.BodyMapAnnotations
            .Where(b => b.VisitId == visitId)
            .OrderBy(b => b.CreatedAt)
            .Select(b => new BodyAnnotationDto(
                b.Id, b.VisitId, b.Region, b.Side,
                b.Symptom, b.PainLevel, b.Notes, b.Diagnosis,
                b.PositionX, b.PositionY, b.MarkerColor, b.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<BodyAnnotationDto> AddBodyAnnotationAsync(
        Guid doctorId, CreateBodyAnnotationRequest req, CancellationToken ct = default)
    {
        var annotation = new BodyMapAnnotation
        {
            Id          = Guid.NewGuid(),
            VisitId     = req.VisitId,
            PatientId   = req.PatientId,
            DoctorId    = doctorId,
            ClinicId    = _tenant.ClinicId,
            Region      = req.Region,
            Side        = req.Side,
            Symptom     = req.Symptom,
            PainLevel   = req.PainLevel,
            Notes       = req.Notes,
            Diagnosis   = req.Diagnosis,
            PositionX   = req.PositionX,
            PositionY   = req.PositionY,
            MarkerColor = req.MarkerColor ?? "#EF4444",
            CreatedAt   = DateTime.UtcNow
        };

        _db.BodyMapAnnotations.Add(annotation);
        await _db.SaveChangesAsync(ct);

        return new BodyAnnotationDto(
            annotation.Id, annotation.VisitId, annotation.Region, annotation.Side,
            annotation.Symptom, annotation.PainLevel, annotation.Notes, annotation.Diagnosis,
            annotation.PositionX, annotation.PositionY, annotation.MarkerColor, annotation.CreatedAt);
    }

    public async Task DeleteBodyAnnotationAsync(
        Guid annotationId, CancellationToken ct = default)
    {
        var a = await _db.BodyMapAnnotations.FindAsync([annotationId], ct)
            ?? throw new KeyNotFoundException("Body annotation not found.");
        _db.BodyMapAnnotations.Remove(a);
        await _db.SaveChangesAsync(ct);
    }

    // ───────────────────────────────────────────────────────────────────
    // Dental
    // ───────────────────────────────────────────────────────────────────

    public async Task<DentalChartDto> GetDentalChartAsync(
        Guid patientId, CancellationToken ct = default)
    {
        var patient = await _db.Patients.FindAsync([patientId], ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        var teeth = await _db.DentalRecords
            .Where(d => d.PatientId == patientId)
            .OrderBy(d => d.ToothNumber)
            .Select(d => new DentalRecordDto(
                d.Id, d.ToothNumber, d.Condition, d.Surface, d.Notes,
                d.TreatmentDate,
                d.Doctor != null ? $"{d.Doctor.User!.FirstName} {d.Doctor.User!.LastName}" : "Unknown",
                d.CreatedAt))
            .ToListAsync(ct);

        return new DentalChartDto(
            patientId,
            $"{patient.FirstName} {patient.LastName}",
            teeth);
    }

    public async Task<DentalRecordDto> UpsertDentalRecordAsync(
        Guid doctorId, UpsertDentalRecordRequest req, CancellationToken ct = default)
    {
        var doctor = await _db.Doctors
            .FirstOrDefaultAsync(d => d.UserId == doctorId && d.ClinicId == _tenant.ClinicId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        // If a record for this tooth already exists in this visit, update it
        var existing = await _db.DentalRecords
            .FirstOrDefaultAsync(d =>
                d.PatientId == req.PatientId &&
                d.ToothNumber == req.ToothNumber &&
                (req.VisitId == null || d.VisitId == req.VisitId), ct);

        if (existing != null)
        {
            existing.Condition     = req.Condition;
            existing.Surface       = req.Surface;
            existing.Notes         = req.Notes;
            existing.TreatmentDate = req.TreatmentDate;
            existing.UpdatedAt     = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return new DentalRecordDto(
                existing.Id, existing.ToothNumber, existing.Condition,
                existing.Surface, existing.Notes, existing.TreatmentDate,
                "Doctor", existing.CreatedAt);
        }

        var record = new DentalRecord
        {
            Id            = Guid.NewGuid(),
            PatientId     = req.PatientId,
            ClinicId      = _tenant.ClinicId,
            VisitId       = req.VisitId,
            DoctorId      = doctor.Id,
            ToothNumber   = req.ToothNumber,
            Condition     = req.Condition,
            Surface       = req.Surface,
            Notes         = req.Notes,
            TreatmentDate = req.TreatmentDate,
            CreatedAt     = DateTime.UtcNow
        };

        _db.DentalRecords.Add(record);
        await _db.SaveChangesAsync(ct);

        return new DentalRecordDto(
            record.Id, record.ToothNumber, record.Condition,
            record.Surface, record.Notes, record.TreatmentDate,
            "Doctor", record.CreatedAt);
    }

    public async Task DeleteDentalRecordAsync(
        Guid recordId, CancellationToken ct = default)
    {
        var r = await _db.DentalRecords.FindAsync([recordId], ct)
            ?? throw new KeyNotFoundException("Dental record not found.");
        _db.DentalRecords.Remove(r);
        await _db.SaveChangesAsync(ct);
    }
}
