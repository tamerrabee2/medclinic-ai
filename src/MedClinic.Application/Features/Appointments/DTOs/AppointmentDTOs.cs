using MedClinic.Domain.Enums;

namespace MedClinic.Application.Features.Appointments.DTOs;

public record AppointmentListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public Guid? DoctorId { get; init; }
    public Guid? PatientId { get; init; }
}

public record CreateAppointmentRequest(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? Type,
    string? Notes);

public record AppointmentPagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
