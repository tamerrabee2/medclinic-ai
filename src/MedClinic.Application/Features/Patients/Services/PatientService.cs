using MedClinic.Application.Features.Patients.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.Patients.Services;

public class PatientService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenant;

    public PatientService(IApplicationDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<PatientPagedResult<Patient>> GetPatientsAsync(PatientListQuery query, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId;
        var q = _context.Patients
            .Where(p => p.ClinicId == clinicId && !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(p => p.FirstName.Contains(s) || p.LastName.Contains(s) || (p.Phone != null && p.Phone.Contains(s)));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PatientPagedResult<Patient>(items, total, query.Page, query.PageSize);
    }

    public async Task<Patient> GetPatientAsync(Guid id, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId;
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId && !p.IsDeleted, ct);

        if (patient == null)
            throw new KeyNotFoundException($"Patient with id '{id}' was not found in clinic.");

        return patient;
    }

    public async Task<Patient> CreatePatientAsync(CreatePatientRequest request, CancellationToken ct = default)
    {
        var clinicId = _tenant.ClinicId ?? Guid.NewGuid();

        Gender gender = Gender.Other;
        if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse<Gender>(request.Gender, true, out var parsedGender))
            gender = parsedGender;

        var patient = new Patient
        {
            ClinicId = clinicId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            Gender = gender,
            Address = request.Address,
            NationalId = request.NationalId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(ct);
        return patient;
    }

    public async Task<Patient> UpdatePatientAsync(Guid id, UpdatePatientRequest request, CancellationToken ct = default)
    {
        var patient = await GetPatientAsync(id, ct);

        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.Phone = request.Phone;
        patient.Email = request.Email;
        patient.Address = request.Address;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return patient;
    }
}
