using FluentAssertions;
using MedClinic.Application.Features.Canvas.DTOs;
using MedClinic.Application.Features.Canvas.Services;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Tests.Unit.Helpers;
using Moq;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class DentalServiceTests
{
    private readonly FakeTenantContext _tenant;
    private readonly Mock<IFileStorage> _storageMock;

    public DentalServiceTests()
    {
        _tenant      = new FakeTenantContext();
        _storageMock = new Mock<IFileStorage>();
    }

    [Fact]
    public async Task UpsertDentalRecord_Creates_NewRecord()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var user    = TestDataBuilder.BuildUser();
        var doctor  = TestDataBuilder.BuildDoctor(
            userId: user.Id, clinicId: _tenant.ClinicId);
        var patient = TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId);

        db.Users.Add(user);
        db.Doctors.Add(doctor);
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        _tenant.UserId = user.Id;
        var service = new CanvasService(db, _tenant, _storageMock.Object);

        var req = new UpsertDentalRecordRequest(
            PatientId:     patient.Id,
            VisitId:       null,
            ToothNumber:   16,
            Condition:     "Cavity",
            Surface:       "Occlusal",
            Notes:         "Needs filling",
            TreatmentDate: DateTime.Today
        );

        // Act
        var result = await service.UpsertDentalRecordAsync(user.Id, req);

        // Assert
        result.Should().NotBeNull();
        result.ToothNumber.Should().Be(16);
        result.Condition.Should().Be("Cavity");
        db.DentalRecords.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpsertDentalRecord_Updates_ExistingRecord()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var user    = TestDataBuilder.BuildUser();
        var doctor  = TestDataBuilder.BuildDoctor(
            userId: user.Id, clinicId: _tenant.ClinicId);
        var patient = TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId);

        db.Users.Add(user);
        db.Doctors.Add(doctor);
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var existing = new DentalRecord
        {
            Id          = Guid.NewGuid(),
            PatientId   = patient.Id,
            ClinicId    = _tenant.ClinicId,
            DoctorId    = doctor.Id,
            ToothNumber = 16,
            Condition   = "Healthy",
            CreatedAt   = DateTime.UtcNow
        };
        db.DentalRecords.Add(existing);
        await db.SaveChangesAsync();

        _tenant.UserId = user.Id;
        var service = new CanvasService(db, _tenant, _storageMock.Object);

        var req = new UpsertDentalRecordRequest(
            PatientId:     patient.Id,
            VisitId:       null,
            ToothNumber:   16,
            Condition:     "Crown",
            Surface:       null,
            Notes:         "Crown placed",
            TreatmentDate: DateTime.Today
        );

        // Act
        var result = await service.UpsertDentalRecordAsync(user.Id, req);

        // Assert
        result.Condition.Should().Be("Crown");
        db.DentalRecords.Should().HaveCount(1); // still 1 record, not 2
    }

    [Fact]
    public async Task GetDentalChart_ReturnsAllTeeth_ForPatient()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var patient = TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId);
        var doctor  = TestDataBuilder.BuildDoctor(clinicId: _tenant.ClinicId);
        db.Patients.Add(patient);
        db.Doctors.Add(doctor);

        db.DentalRecords.AddRange(
            new() { Id = Guid.NewGuid(), PatientId = patient.Id, ClinicId = _tenant.ClinicId,
                    DoctorId = doctor.Id, ToothNumber = 11, Condition = "Healthy",
                    CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), PatientId = patient.Id, ClinicId = _tenant.ClinicId,
                    DoctorId = doctor.Id, ToothNumber = 21, Condition = "Filling",
                    CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new CanvasService(db, _tenant, _storageMock.Object);

        // Act
        var chart = await service.GetDentalChartAsync(patient.Id);

        // Assert
        chart.Teeth.Should().HaveCount(2);
        chart.Teeth.Select(t => t.ToothNumber).Should().BeEquivalentTo([11, 21]);
    }
}
