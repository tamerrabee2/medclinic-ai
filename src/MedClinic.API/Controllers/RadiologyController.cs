using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
public class RadiologyController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public RadiologyController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant  = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ─────────────────────────────────────────────────────────────────────
    // LIST
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>List radiology studies with filters</summary>
    [HttpGet]
    [HasPermission(Permissions.RadiologyRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?   patientId,
        [FromQuery] Guid?   doctorId,
        [FromQuery] string? studyType,
        [FromQuery] string? status,
        [FromQuery] bool?   isAIAnalyzed,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.RadiologyStudies
            .Where(s => s.ClinicId == clinicId)
            .Include(s => s.Patient)
            .AsQueryable();

        if (patientId.HasValue)  query = query.Where(s => s.PatientId == patientId);
        if (doctorId.HasValue)   query = query.Where(s => s.DoctorId  == doctorId);
        if (isAIAnalyzed.HasValue) query = query.Where(s => s.IsAIAnalyzed == isAIAnalyzed);
        if (!string.IsNullOrWhiteSpace(studyType))
            query = query.Where(s => s.StudyType.Contains(studyType));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status.ToString() == status);
        if (from.HasValue) query = query.Where(s => s.StudyDate >= from);
        if (to.HasValue)   query = query.Where(s => s.StudyDate <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.StudyDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.StudyType,
                s.BodyPart,
                s.StudyDate,
                s.Status,
                s.IsAIAnalyzed,
                s.DoctorReviewed,
                s.AccessionNumber,
                Patient = new { s.Patient.Id, s.Patient.FirstName, s.Patient.LastName }
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items      = items.Cast<object>().ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Get radiology study with images and AI analyses</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.RadiologyRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var study = await _context.RadiologyStudies
            .Where(s => s.Id == id && s.ClinicId == clinicId)
            .Include(s => s.Patient)
            .Include(s => s.Images)
            .Include(s => s.AIAnalyses)
            .FirstOrDefaultAsync(ct);

        if (study == null) return NotFound("Radiology study not found.");

        return Success(new
        {
            study.Id,
            study.StudyType,
            study.BodyPart,
            study.StudyDate,
            study.Status,
            study.ClinicalInfo,
            study.Findings,
            study.Impression,
            study.Notes,
            study.ReportedBy,
            study.ReportedAt,
            study.AccessionNumber,
            study.IsAIAnalyzed,
            study.DoctorReviewed,
            study.DoctorId,
            study.VisitId,
            Patient = new
            {
                study.Patient.Id,
                study.Patient.FirstName,
                study.Patient.LastName,
                study.Patient.DateOfBirth
            },
            Images = study.Images.Select(i => new
            {
                i.Id,
                i.FileName,
                i.FileUrl,
                i.Modality,
                i.SeriesNumber,
                i.InstanceNumber,
                i.UploadedAt
            }),
            AIAnalyses = study.AIAnalyses.Select(a => new
            {
                a.Id,
                a.AnalysisType,
                a.Findings,
                a.Confidence,
                a.AnalyzedAt,
                a.ModelVersion
            })
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Create a new radiology study order</summary>
    [HttpPost]
    [HasPermission(Permissions.RadiologyCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRadiologyStudyRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId && p.ClinicId == clinicId, ct);
        if (!patientExists) return NotFound("Patient not found.");

        // Generate accession number: RAD-{ClinicId[..4]}-{timestamp}
        var accession = $"RAD-{clinicId.ToString()[..4].ToUpper()}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var study = new RadiologyStudy
        {
            ClinicId        = clinicId,
            PatientId       = request.PatientId,
            DoctorId        = request.DoctorId,
            VisitId         = request.VisitId,
            StudyType       = request.StudyType,
            BodyPart        = request.BodyPart,
            StudyDate       = request.StudyDate ?? DateTime.UtcNow,
            ClinicalInfo    = request.ClinicalInfo,
            Notes           = request.Notes,
            AccessionNumber = accession,
            Status          = RadiologyStudyStatus.Pending,
            CreatedBy       = CurrentUserId
        };

        _context.RadiologyStudies.Add(study);
        await _context.SaveChangesAsync(ct);

        return Created(new
        {
            study.Id,
            study.StudyType,
            study.BodyPart,
            study.AccessionNumber,
            study.Status
        }, "Radiology study created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // REPORT (Radiologist writes findings)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Radiologist submits report (Findings + Impression)</summary>
    [HttpPost("{id:guid}/report")]
    [HasPermission(Permissions.RadiologyReport)]
    public async Task<IActionResult> SubmitReport(
        Guid id,
        [FromBody] SubmitReportRequest request,
        CancellationToken ct)
    {
        var study = await GetOwnedStudy(id, ct);
        if (study == null) return NotFound("Radiology study not found.");
        if (study.Status == RadiologyStudyStatus.Cancelled)
            return BadRequest("Cannot report a cancelled study.");

        study.Findings    = request.Findings;
        study.Impression  = request.Impression;
        study.ReportedBy  = request.ReportedBy;
        study.ReportedAt  = DateTime.UtcNow;
        study.Status      = RadiologyStudyStatus.Completed;
        study.UpdatedBy   = CurrentUserId;

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Report submitted.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // DOCTOR REVIEW
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Doctor marks study as reviewed</summary>
    [HttpPost("{id:guid}/review")]
    [HasPermission(Permissions.RadiologyUpdate)]
    public async Task<IActionResult> MarkReviewed(Guid id, CancellationToken ct)
    {
        var study = await GetOwnedStudy(id, ct);
        if (study == null) return NotFound("Radiology study not found.");

        study.DoctorReviewed = true;
        study.UpdatedBy      = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Study marked as reviewed by doctor.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // STATUS UPDATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Update study status (InProgress / Cancelled)</summary>
    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.RadiologyUpdate)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateRadiologyStatusRequest request,
        CancellationToken ct)
    {
        var study = await GetOwnedStudy(id, ct);
        if (study == null) return NotFound("Radiology study not found.");
        if (study.Status == RadiologyStudyStatus.Completed)
            return BadRequest("Cannot change status of a completed study.");

        if (!Enum.TryParse<RadiologyStudyStatus>(request.Status, true, out var newStatus))
            return BadRequest($"Invalid status '{request.Status}'.");

        study.Status    = newStatus;
        study.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, $"Study status updated to {newStatus}.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // PATIENT RADIOLOGY HISTORY
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Get radiology history for a patient</summary>
    [HttpGet("/api/v1/patients/{patientId:guid}/radiology-history")]
    [HasPermission(Permissions.RadiologyRead)]
    public async Task<IActionResult> GetPatientRadiologyHistory(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 50);
        var clinicId = ClinicId;

        var total = await _context.RadiologyStudies
            .CountAsync(s => s.PatientId == patientId && s.ClinicId == clinicId, ct);

        var studies = await _context.RadiologyStudies
            .Where(s => s.PatientId == patientId && s.ClinicId == clinicId)
            .Include(s => s.Images)
            .Include(s => s.AIAnalyses)
            .OrderByDescending(s => s.StudyDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.StudyType,
                s.BodyPart,
                s.StudyDate,
                s.Status,
                s.Impression,
                s.IsAIAnalyzed,
                s.DoctorReviewed,
                s.AccessionNumber,
                ImageCount     = s.Images.Count,
                AIAnalysisCount = s.AIAnalyses.Count
            })
            .ToListAsync(ct);

        return Success(new
        {
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            Items      = studies
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────────────

    private async Task<RadiologyStudy?> GetOwnedStudy(Guid id, CancellationToken ct)
        => await _context.RadiologyStudies
            .FirstOrDefaultAsync(s => s.Id == id && s.ClinicId == ClinicId, ct);
}

// ── DTOs ─────────────────────────────────────────────────────────────────

public record CreateRadiologyStudyRequest(
    Guid      PatientId,
    Guid?     DoctorId,
    Guid?     VisitId,
    string    StudyType,        // X-Ray, CT, MRI, Ultrasound, PET, Mammography
    string?   BodyPart,
    DateTime? StudyDate,
    string?   ClinicalInfo,
    string?   Notes);

public record SubmitReportRequest(
    string  Findings,
    string  Impression,
    string  ReportedBy);

public record UpdateRadiologyStatusRequest(string Status);
