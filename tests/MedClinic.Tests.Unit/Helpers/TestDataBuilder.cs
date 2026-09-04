using MedClinic.Domain.Entities;

namespace MedClinic.Tests.Unit.Helpers;

public static class TestDataBuilder
{
    public static User BuildUser(
        Guid? id = null,
        string email = "doctor@test.com",
        string firstName = "Ahmed",
        string lastName  = "Hassan") => new()
    {
        Id        = id ?? Guid.NewGuid(),
        Email     = email,
        FirstName = firstName,
        LastName  = lastName,
        IsActive  = true,
        CreatedAt = DateTime.UtcNow
    };

    public static Clinic BuildClinic(
        Guid? id = null,
        string name = "Test Clinic") => new()
    {
        Id        = id ?? Guid.NewGuid(),
        Name      = name,
        IsActive  = true,
        CreatedAt = DateTime.UtcNow
    };

    public static Patient BuildPatient(
        Guid? id = null,
        Guid? clinicId = null,
        string firstName = "Sara",
        string lastName  = "Mohamed",
        string? phone    = "0501234567") => new()
    {
        Id          = id ?? Guid.NewGuid(),
        ClinicId    = clinicId ?? Guid.NewGuid(),
        FirstName   = firstName,
        LastName    = lastName,
        Phone       = phone,
        DateOfBirth = new DateTime(1990, 1, 1),
        Gender      = "Female",
        IsActive    = true,
        CreatedAt   = DateTime.UtcNow
    };

    public static Appointment BuildAppointment(
        Guid? id = null,
        Guid? clinicId  = null,
        Guid? patientId = null,
        Guid? doctorId  = null,
        DateTime? scheduledAt = null) => new()
    {
        Id          = id ?? Guid.NewGuid(),
        ClinicId    = clinicId  ?? Guid.NewGuid(),
        PatientId   = patientId ?? Guid.NewGuid(),
        DoctorId    = doctorId  ?? Guid.NewGuid(),
        ScheduledAt = scheduledAt ?? DateTime.UtcNow.AddDays(1),
        Status      = "Scheduled",
        CreatedAt   = DateTime.UtcNow
    };

    public static Doctor BuildDoctor(
        Guid? id = null,
        Guid? userId   = null,
        Guid? clinicId = null,
        string specialty = "General Medicine") => new()
    {
        Id         = id ?? Guid.NewGuid(),
        UserId     = userId   ?? Guid.NewGuid(),
        ClinicId   = clinicId ?? Guid.NewGuid(),
        Specialty  = specialty,
        IsActive   = true,
        CreatedAt  = DateTime.UtcNow
    };

    public static LabResult BuildLabResult(
        Guid? id = null,
        Guid? orderId = null) => new()
    {
        Id         = id ?? Guid.NewGuid(),
        OrderId    = orderId ?? Guid.NewGuid(),
        ResultDate = DateTime.UtcNow,
        Status     = "Final",
        CreatedAt  = DateTime.UtcNow,
        Items      = []
    };
}
