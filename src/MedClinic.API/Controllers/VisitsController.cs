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
public class VisitsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public VisitsController(ApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ─────────────────────────────────────────────────────────────────────
    // LIST
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>List visits with filters and pagination</summary>
    [HttpGet]
    [HasPermission(Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? doctorId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 100);
        var clinicId = ClinicId;

        var query = _context.Visits
            .Where(v => v.ClinicId == clinicId)
            .Include(v => v.Patient)
            .Include(v => v.Doctor).ThenInclude(d => d.User)
            .AsQueryable();

        if (patientId.HasValue) query = query.Where(v => v.PatientId == patientId);
        if (doctorId.HasValue)  query = query.Where(v => v.DoctorId  == doctorId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status.ToString() == status);
        if (from.HasValue) query = query.Where(v => v.VisitDate >= from);
        if (to.HasValue)   query = query.Where(v => v.VisitDate <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.VisitDate,
                v.Status,
                v.ChiefComplaint,
                v.Diagnosis,
                Patient = new { v.Patient.Id, v.Patient.FirstName, v.Patient.LastName },
                Doctor  = new
                {
                    v.Doctor.Id,
                    Name = v.Doctor.User.FirstName + " " + v.Doctor.User.LastName,
                    v.Doctor.Specialty
                }
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items = items.Cast<object>().ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // GET BY ID (full detail)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Get full visit detail</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var visit = await _context.Visits
            .Where(v => v.Id == id && v.ClinicId == clinicId)
            .Include(v => v.Patient)
            .Include(v => v.Doctor).ThenInclude(d => d.User)
            .Include(v => v.Appointment)
            .Include(v => v.Prescriptions).ThenInclude(p => p.Items)
            .Include(v => v.LabOrders).ThenInclude(lo => lo.Results)
            .FirstOrDefaultAsync(ct);

        if (visit == null) return NotFound("Visit not found.");

        var result = new
        {
            visit.Id,
            visit.VisitDate,
            visit.Status,
            visit.ChiefComplaint,
            visit.Symptoms,
            visit.PhysicalExamination,
            visit.Diagnosis,
            visit.DifferentialDiagnosis,
            visit.TreatmentPlan,
            visit.DoctorNotes,
            visit.FollowUpNotes,
            visit.FollowUpDate,
            Vitals = visit.Vitals == null ? null : new
            {
                visit.Vitals.Temperature,
                visit.Vitals.BloodPressureSystolic,
                visit.Vitals.BloodPressureDiastolic,
                visit.Vitals.HeartRate,
                visit.Vitals.RespiratoryRate,
                visit.Vitals.OxygenSaturation,
                visit.Vitals.Weight,
                visit.Vitals.Height,
                visit.Vitals.BMI
            },
            Patient = new
            {
                visit.Patient.Id,
                visit.Patient.FirstName,
                visit.Patient.LastName,
                visit.Patient.DateOfBirth,
                visit.Patient.BloodType,
                visit.Patient.Allergies
            },
            Doctor = new
            {
                visit.Doctor.Id,
                Name = visit.Doctor.User.FirstName + " " + visit.Doctor.User.LastName,
                visit.Doctor.Specialty
            },
            AppointmentId = visit.AppointmentId,
            PrescriptionCount = visit.Prescriptions.Count,
            LabOrderCount     = visit.LabOrders.Count
        };

        return Success(result);
    }

    // ─────────────────────────────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Start a new visit (open visit)</summary>
    [HttpPost]
    [HasPermission(Permissions.MedicalRecordsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateVisitRequest request, CancellationToken ct)
    {
        var clinicId = ClinicId;

        // Validate patient & doctor belong to clinic
        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == request.PatientId && p.ClinicId == clinicId, ct);
        if (!patientExists) return NotFound("Patient not found in this clinic.");

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId && d.ClinicId == clinicId, ct);
        if (!doctorExists) return NotFound("Doctor not found in this clinic.");

        // Check no open visit already exists for this patient
        var openVisit = await _context.Visits.AnyAsync(v =>
            v.PatientId == request.PatientId &&
            v.ClinicId  == clinicId &&
            v.Status    == VisitStatus.InProgress, ct);
        if (openVisit) return BadRequest("Patient already has an open visit in progress.");

        var visit = new Visit
        {
            ClinicId        = clinicId,
            PatientId       = request.PatientId,
            DoctorId        = request.DoctorId,
            AppointmentId   = request.AppointmentId,
            VisitDate       = request.VisitDate ?? DateTime.UtcNow,
            ChiefComplaint  = request.ChiefComplaint,
            Symptoms        = request.Symptoms,
            Status          = VisitStatus.InProgress,
            CreatedBy       = CurrentUserId
        };

        // Record vitals if provided
        if (request.Vitals != null)
            visit.Vitals = MapVitals(request.Vitals);

        // Auto-link appointment status
        if (request.AppointmentId.HasValue)
        {
            var apt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.ClinicId == clinicId, ct);
            if (apt != null)
            {
                apt.Status    = "InProgress";
                apt.UpdatedBy = CurrentUserId;
            }
        }

        _context.Visits.Add(visit);
        await _context.SaveChangesAsync(ct);

        return Created(new { visit.Id, visit.VisitDate, visit.Status }, "Visit started.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // UPDATE (partial — doctors fill in as visit progresses)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Update visit notes, diagnosis, treatment, vitals</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVisitRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var visit = await _context.Visits
            .FirstOrDefaultAsync(v => v.Id == id && v.ClinicId == clinicId, ct);

        if (visit == null) return NotFound("Visit not found.");
        if (visit.Status == VisitStatus.Cancelled)
            return BadRequest("Cannot update a cancelled visit.");

        if (request.ChiefComplaint      != null) visit.ChiefComplaint      = request.ChiefComplaint;
        if (request.Symptoms            != null) visit.Symptoms            = request.Symptoms;
        if (request.PhysicalExamination != null) visit.PhysicalExamination = request.PhysicalExamination;
        if (request.Diagnosis           != null) visit.Diagnosis           = request.Diagnosis;
        if (request.DifferentialDiagnosis != null) visit.DifferentialDiagnosis = request.DifferentialDiagnosis;
        if (request.TreatmentPlan       != null) visit.TreatmentPlan       = request.TreatmentPlan;
        if (request.DoctorNotes         != null) visit.DoctorNotes         = request.DoctorNotes;
        if (request.FollowUpNotes       != null) visit.FollowUpNotes       = request.FollowUpNotes;
        if (request.FollowUpDate.HasValue)        visit.FollowUpDate        = request.FollowUpDate;
        if (request.Vitals              != null)  visit.Vitals              = MapVitals(request.Vitals);

        visit.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Visit updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // COMPLETE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Complete / close a visit</summary>
    [HttpPost("{id:guid}/complete")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteVisitRequest? request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var visit = await _context.Visits
            .Include(v => v.Appointment)
            .FirstOrDefaultAsync(v => v.Id == id && v.ClinicId == clinicId, ct);

        if (visit == null) return NotFound("Visit not found.");
        if (visit.Status == VisitStatus.Completed)
            return BadRequest("Visit is already completed.");
        if (visit.Status == VisitStatus.Cancelled)
            return BadRequest("Cannot complete a cancelled visit.");

        visit.Status         = VisitStatus.Completed;
        visit.FollowUpNotes  = request?.FollowUpNotes ?? visit.FollowUpNotes;
        visit.FollowUpDate   = request?.FollowUpDate  ?? visit.FollowUpDate;
        visit.UpdatedBy      = CurrentUserId;

        // Auto-complete linked appointment
        if (visit.Appointment != null && visit.Appointment.Status != "Cancelled")
        {
            visit.Appointment.Status    = "Completed";
            visit.Appointment.UpdatedBy = CurrentUserId;
        }

        await _context.SaveChangesAsync(ct);
        return Success<object>(null!, "Visit completed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // CANCEL
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Cancel a visit</summary>
    [HttpPost("{id:guid}/cancel")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var clinicId = ClinicId;
        var visit = await _context.Visits
            .FirstOrDefaultAsync(v => v.Id == id && v.ClinicId == clinicId, ct);

        if (visit == null) return NotFound("Visit not found.");
        if (visit.Status == VisitStatus.Completed)
            return BadRequest("Cannot cancel a completed visit.");

        visit.Status    = VisitStatus.Cancelled;
        visit.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Visit cancelled.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // PATIENT HISTORY
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Get full medical history timeline for a patient</summary>
    [HttpGet("patient/{patientId:guid}/history")]
    [HasPermission(Permissions.MedicalRecordsRead)]
    public async Task<IActionResult> GetPatientHistory(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, 50);
        var clinicId = ClinicId;

        var total = await _context.Visits
            .CountAsync(v => v.PatientId == patientId && v.ClinicId == clinicId, ct);

        var history = await _context.Visits
            .Where(v => v.PatientId == patientId && v.ClinicId == clinicId)
            .Include(v => v.Doctor).ThenInclude(d => d.User)
            .Include(v => v.Prescriptions)
            .Include(v => v.LabOrders)
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.VisitDate,
                v.Status,
                v.ChiefComplaint,
                v.Diagnosis,
                v.TreatmentPlan,
                v.FollowUpDate,
                Doctor = new
                {
                    v.Doctor.Id,
                    Name = v.Doctor.User.FirstName + " " + v.Doctor.User.LastName,
                    v.Doctor.Specialty
                },
                PrescriptionCount = v.Prescriptions.Count,
                LabOrderCount     = v.LabOrders.Count
            })
            .ToListAsync(ct);

        return Success(new PagedResult<object>
        {
            Items      = history.Cast<object>().ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // VITALS — standalone update
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Update or record vitals for an existing visit</summary>
    [HttpPatch("{id:guid}/vitals")]
    [HasPermission(Permissions.MedicalRecordsUpdate)]
    public async Task<IActionResult> UpdateVitals(
        Guid id,
        [FromBody] VitalsRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;
        var visit = await _context.Visits
            .FirstOrDefaultAsync(v => v.Id == id && v.ClinicId == clinicId, ct);

        if (visit == null) return NotFound("Visit not found.");

        visit.Vitals    = MapVitals(request);
        visit.UpdatedBy = CurrentUserId;
        await _context.SaveChangesAsync(ct);

        return Success(visit.Vitals, "Vitals updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────────────
    private static Vitals MapVitals(VitalsRequest r) => new()
    {
        Temperature              = r.Temperature,
        BloodPressureSystolic    = r.BloodPressureSystolic,
        BloodPressureDiastolic   = r.BloodPressureDiastolic,
        HeartRate                = r.HeartRate,
        RespiratoryRate          = r.RespiratoryRate,
        OxygenSaturation         = r.OxygenSaturation,
        Weight                   = r.Weight,
        Height                   = r.Height,
        BMI = r.Weight.HasValue && r.Height.HasValue && r.Height > 0
            ? Math.Round(r.Weight.Value / (r.Height.Value / 100 * r.Height.Value / 100), 2)
            : r.BMI
    };
}

// ── DTOs ────────────────────────────────────────────────────────────────────

public record VitalsRequest(
    decimal? Temperature,
    int?     BloodPressureSystolic,
    int?     BloodPressureDiastolic,
    int?     HeartRate,
    int?     RespiratoryRate,
    decimal? OxygenSaturation,
    decimal? Weight,
    decimal? Height,
    decimal? BMI);

public record CreateVisitRequest(
    Guid        PatientId,
    Guid        DoctorId,
    Guid?       AppointmentId,
    DateTime?   VisitDate,
    string?     ChiefComplaint,
    string?     Symptoms,
    VitalsRequest? Vitals);

public record UpdateVisitRequest(
    string?     ChiefComplaint,
    string?     Symptoms,
    string?     PhysicalExamination,
    string?     Diagnosis,
    string?     DifferentialDiagnosis,
    string?     TreatmentPlan,
    string?     DoctorNotes,
    string?     FollowUpNotes,
    DateTime?   FollowUpDate,
    VitalsRequest? Vitals);

public record CompleteVisitRequest(
    string?   FollowUpNotes,
    DateTime? FollowUpDate);
