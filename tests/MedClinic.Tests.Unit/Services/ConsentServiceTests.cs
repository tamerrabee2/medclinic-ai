using FluentAssertions;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedClinic.Tests.Unit.Services;

public class ConsentServiceTests
{
    private readonly FakeTenantContext _tenant;

    public ConsentServiceTests()
    {
        _tenant = new FakeTenantContext();
    }

    [Fact]
    public async Task ConsentRecord_Creation_StoresGrantedStatusAndType()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            FirstName = "Consent",
            LastName = "Patient",
            DateOfBirth = new DateTime(1985, 5, 5),
            CreatedAt = DateTime.UtcNow
        };
        db.Patients.Add(patient);

        var consent = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            PatientId = patient.Id,
            ConsentType = ConsentType.AiAssistedCare,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            Notes = "Patient consented to Dr. AI decision support assistance.",
            CreatedAt = DateTime.UtcNow
        };
        db.ConsentRecords.Add(consent);
        await db.SaveChangesAsync();

        // Act
        var stored = await db.ConsentRecords
            .Include(c => c.Patient)
            .FirstOrDefaultAsync(c => c.Id == consent.Id);

        // Assert
        stored.Should().NotBeNull();
        stored!.ConsentType.Should().Be(ConsentType.AiAssistedCare);
        stored.IsGranted.Should().BeTrue();
        stored.Patient.FirstName.Should().Be("Consent");
        stored.Notes.Should().Contain("Dr. AI");
    }

    [Fact]
    public async Task ConsentRecord_Revocation_SetsIsGrantedFalse()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            FirstName = "Consent2",
            LastName = "Patient2",
            DateOfBirth = new DateTime(1990, 1, 1),
            CreatedAt = DateTime.UtcNow
        };
        db.Patients.Add(patient);

        var consent = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ClinicId = _tenant.ClinicId,
            PatientId = patient.Id,
            ConsentType = ConsentType.DataSharing,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        db.ConsentRecords.Add(consent);
        await db.SaveChangesAsync();

        // Act — Patient revokes consent
        consent.IsGranted = false;
        consent.Notes = "Revoked by patient request";
        await db.SaveChangesAsync();

        // Assert
        var updated = await db.ConsentRecords.FindAsync(consent.Id);
        updated.Should().NotBeNull();
        updated!.IsGranted.Should().BeFalse();
        updated.Notes.Should().Be("Revoked by patient request");
    }
}
