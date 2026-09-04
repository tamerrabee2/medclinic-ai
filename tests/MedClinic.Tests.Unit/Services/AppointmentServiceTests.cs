using FluentAssertions;
using MedClinic.Application.Features.Appointments.Services;
using MedClinic.Tests.Unit.Helpers;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class AppointmentServiceTests
{
    private readonly FakeTenantContext _tenant;

    public AppointmentServiceTests()
    {
        _tenant = new FakeTenantContext();
    }

    [Fact]
    public async Task GetAppointmentsAsync_ReturnsOnlyClinicAppointments()
    {
        // Arrange
        var db       = TestDbContextFactory.Create();
        var clinicB  = Guid.NewGuid();

        db.Appointments.AddRange(
            TestDataBuilder.BuildAppointment(clinicId: _tenant.ClinicId),
            TestDataBuilder.BuildAppointment(clinicId: clinicB)
        );
        await db.SaveChangesAsync();

        var service = new AppointmentService(db, _tenant);

        // Act
        var result = await service.GetAppointmentsAsync(
            new() { Page = 1, PageSize = 20 });

        // Assert
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAppointmentAsync_FailsForDoubleBooking()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var doctor  = TestDataBuilder.BuildDoctor(clinicId: _tenant.ClinicId);
        var patient = TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId);
        db.Doctors.Add(doctor);
        db.Patients.Add(patient);

        var scheduledAt = DateTime.UtcNow.AddDays(1).Date.AddHours(10);

        var existing = TestDataBuilder.BuildAppointment(
            clinicId:  _tenant.ClinicId,
            doctorId:  doctor.Id,
            patientId: patient.Id,
            scheduledAt: scheduledAt);
        db.Appointments.Add(existing);
        await db.SaveChangesAsync();

        var service = new AppointmentService(db, _tenant);
        var req = new CreateAppointmentRequest(
            PatientId:   patient.Id,
            DoctorId:    doctor.Id,
            ScheduledAt: scheduledAt,
            DurationMinutes: 30,
            Type:   "Regular",
            Notes:  null
        );

        // Act
        var act = async () => await service.CreateAppointmentAsync(req);

        // Assert — double-booking same doctor at same time must be rejected
        await act.Should().ThrowAsync<InvalidOperationException>
            ("because doctor is already booked at that time slot");
    }

    [Fact]
    public async Task CancelAppointmentAsync_UpdatesStatusToCancelled()
    {
        // Arrange
        var db   = TestDbContextFactory.Create();
        var appt = TestDataBuilder.BuildAppointment(clinicId: _tenant.ClinicId);
        appt.Status = "Scheduled";
        db.Appointments.Add(appt);
        await db.SaveChangesAsync();

        var service = new AppointmentService(db, _tenant);

        // Act
        await service.CancelAppointmentAsync(appt.Id, "Patient request");

        // Assert
        var updated = await db.Appointments.FindAsync(appt.Id);
        updated!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CompleteAppointmentAsync_SetsStatusToCompleted()
    {
        // Arrange
        var db   = TestDbContextFactory.Create();
        var appt = TestDataBuilder.BuildAppointment(clinicId: _tenant.ClinicId);
        appt.Status = "Scheduled";
        db.Appointments.Add(appt);
        await db.SaveChangesAsync();

        var service = new AppointmentService(db, _tenant);

        // Act
        await service.CompleteAppointmentAsync(appt.Id);

        // Assert
        var updated = await db.Appointments.FindAsync(appt.Id);
        updated!.Status.Should().Be("Completed");
    }
}
