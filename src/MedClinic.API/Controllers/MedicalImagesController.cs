using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Constants;
using MedClinic.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[Authorize]
[Route("api/v1/radiology/{studyId:guid}/images")]
public class MedicalImagesController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext       _tenant;
    private readonly IFileStorage         _fileStorage;

    public MedicalImagesController(
        ApplicationDbContext context,
        ITenantContext tenant,
        IFileStorage fileStorage)
    {
        _context     = context;
        _tenant      = tenant;
        _fileStorage = fileStorage;
    }

    private Guid ClinicId => _tenant.ClinicId
        ?? throw new UnauthorizedAccessException("Clinic context required.");

    // ─────────────────────────────────────────────────────────────────────
    // UPLOAD IMAGE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Upload a medical image for a radiology study</summary>
    [HttpPost]
    [HasPermission(Permissions.RadiologyCreate)]
    [RequestSizeLimit(52_428_800)] // 50 MB per image
    public async Task<IActionResult> Upload(
        Guid studyId,
        [FromForm] UploadMedicalImageRequest request,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var studyExists = await _context.RadiologyStudies
            .AnyAsync(s => s.Id == studyId && s.ClinicId == clinicId, ct);
        if (!studyExists) return NotFound("Radiology study not found.");

        // Validate file
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/dicom", "application/dicom", "image/tiff" };
        if (!allowedTypes.Contains(request.File.ContentType.ToLower()))
            return BadRequest("Invalid file type. Allowed: JPEG, PNG, DICOM, TIFF.");

        // Save file using IFileStorage
        var folder   = $"radiology/{clinicId}/{studyId}";
        var fileUrl  = await _fileStorage.SaveAsync(request.File, folder, ct);

        var image = new MedicalImage
        {
            RadiologyStudyId = studyId,
            FileName         = request.File.FileName,
            FileUrl          = fileUrl,
            FileSizeBytes    = request.File.Length,
            ContentType      = request.File.ContentType,
            Modality         = request.Modality,
            SeriesNumber     = request.SeriesNumber,
            InstanceNumber   = request.InstanceNumber,
            UploadedAt       = DateTime.UtcNow,
            CreatedBy        = CurrentUserId
        };

        _context.MedicalImages.Add(image);

        // Auto-set study to InProgress if still Pending
        var study = await _context.RadiologyStudies.FindAsync([studyId], ct);
        if (study is { Status: RadiologyStudyStatus.Pending })
        {
            study.Status    = RadiologyStudyStatus.InProgress;
            study.UpdatedBy = CurrentUserId;
        }

        await _context.SaveChangesAsync(ct);

        return Created(new
        {
            image.Id,
            image.FileName,
            image.FileUrl,
            image.FileSizeBytes,
            image.Modality
        }, "Image uploaded.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // LIST IMAGES
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>List all images for a study</summary>
    [HttpGet]
    [HasPermission(Permissions.RadiologyRead)]
    public async Task<IActionResult> GetImages(Guid studyId, CancellationToken ct)
    {
        var clinicId = ClinicId;

        var studyExists = await _context.RadiologyStudies
            .AnyAsync(s => s.Id == studyId && s.ClinicId == clinicId, ct);
        if (!studyExists) return NotFound("Radiology study not found.");

        var images = await _context.MedicalImages
            .Where(i => i.RadiologyStudyId == studyId)
            .OrderBy(i => i.SeriesNumber).ThenBy(i => i.InstanceNumber)
            .Select(i => new
            {
                i.Id,
                i.FileName,
                i.FileUrl,
                i.FileSizeBytes,
                i.ContentType,
                i.Modality,
                i.SeriesNumber,
                i.InstanceNumber,
                i.UploadedAt
            })
            .ToListAsync(ct);

        return Success(images);
    }

    // ─────────────────────────────────────────────────────────────────────
    // DELETE IMAGE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Delete a medical image from study and storage</summary>
    [HttpDelete("{imageId:guid}")]
    [HasPermission(Permissions.RadiologyUpdate)]
    public async Task<IActionResult> DeleteImage(
        Guid studyId,
        Guid imageId,
        CancellationToken ct)
    {
        var clinicId = ClinicId;

        var studyExists = await _context.RadiologyStudies
            .AnyAsync(s => s.Id == studyId && s.ClinicId == clinicId, ct);
        if (!studyExists) return NotFound("Radiology study not found.");

        var image = await _context.MedicalImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.RadiologyStudyId == studyId, ct);
        if (image == null) return NotFound("Image not found.");

        // Delete from storage
        await _fileStorage.DeleteAsync(image.FileUrl, ct);

        _context.MedicalImages.Remove(image);
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Image deleted.");
    }
}

// ── DTOs ────────────────────────────────────────────────────────────────

public class UploadMedicalImageRequest
{
    public IFormFile File           { get; set; } = null!;
    public string?   Modality       { get; set; } // X-Ray, CT, MRI...
    public int?      SeriesNumber   { get; set; }
    public int?      InstanceNumber { get; set; }
}
