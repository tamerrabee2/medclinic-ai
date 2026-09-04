using FluentAssertions;
using MedClinic.Application.Features.Patients.Services;
using MedClinic.Tests.Unit.Helpers;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class PatientServiceTests
{
    private readonly FakeTenantContext _tenant;

    public PatientServiceTests()
    {
        _tenant = new FakeTenantContext();
    }

    [Fact]
    public async Task GetPatientsAsync_ReturnsOnlyClinicPatients()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var clinicA = _tenant.ClinicId;
        var clinicB = Guid.NewGuid();

        var patientA = TestDataBuilder.BuildPatient(clinicId: clinicA, firstName: "Ali",  lastName: "Saleh");
        var patientB = TestDataBuilder.BuildPatient(clinicId: clinicB, firstName: "Omar", lastName: "Khalid");
        db.Patients.AddRange(patientA, patientB);
        await db.SaveChangesAsync();

        var service = new PatientService(db, _tenant);

        // Act
        var result = await service.GetPatientsAsync(new() { Page = 1, PageSize = 20 });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].FirstName.Should().Be("Ali");
    }

    [Fact]
    public async Task CreatePatientAsync_SavesPatientWithCorrectClinic()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var service = new PatientService(db, _tenant);
        var request = new CreatePatientRequest(
            FirstName: "Nora",
            LastName:  "Ahmad",
            Phone:     "0509876543",
            Email:     "nora@test.com",
            DateOfBirth: new DateTime(1995, 6, 15),
            Gender:    "Female",
            Address:   null,
            NationalId: null
        );

        // Act
        var result = await service.CreatePatientAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Nora");
        result.LastName.Should().Be("Ahmad");

        var saved = await db.Patients.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.ClinicId.Should().Be(_tenant.ClinicId);
    }

    [Fact]
    public async Task GetPatientAsync_ThrowsKeyNotFound_WhenPatientInOtherClinic()
    {
        // Arrange
        var db         = TestDbContextFactory.Create();
        var otherClinic = Guid.NewGuid();
        var patient    = TestDataBuilder.BuildPatient(clinicId: otherClinic);
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var service = new PatientService(db, _tenant);

        // Act
        var act = async () => await service.GetPatientAsync(patient.Id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SearchPatientsAsync_FindsByName()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        db.Patients.AddRange(
            TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId, firstName: "Khalid", lastName: "Hassan"),
            TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId, firstName: "Mona",   lastName: "Ali")
        );
        await db.SaveChangesAsync();

        var service = new PatientService(db, _tenant);

        // Act
        var result = await service.GetPatientsAsync(
            new() { Page = 1, PageSize = 20, Search = "Khalid" });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].FirstName.Should().Be("Khalid");
    }

    [Fact]
    public async Task UpdatePatientAsync_UpdatesFields()
    {
        // Arrange
        var db      = TestDbContextFactory.Create();
        var patient = TestDataBuilder.BuildPatient(clinicId: _tenant.ClinicId, firstName: "Old");
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var service = new PatientService(db, _tenant);
        var request = new UpdatePatientRequest(
            FirstName: "New",
            LastName:  patient.LastName,
            Phone:     patient.Phone,
            Email:     null,
            Address:   null
        );

        // Act
        var result = await service.UpdatePatientAsync(patient.Id, request);

        // Assert
        result.FirstName.Should().Be("New");
    }
}
