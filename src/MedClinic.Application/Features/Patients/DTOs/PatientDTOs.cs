using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;

namespace MedClinic.Application.Features.Patients.DTOs;

public record PatientListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
}

public record PatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    DateTime DateOfBirth,
    Gender Gender,
    string? NationalId,
    BloodType BloodType,
    DateTime CreatedAt);

public record CreatePatientRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    DateTime DateOfBirth,
    string Gender,
    string? Address,
    string? NationalId);

public record UpdatePatientRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    string? Address);

public record PatientPagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
