using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class FhirSyncTests
{
    [Fact]
    public void FhirSyncRecord_DefaultDirection_ShouldBeExport()
    {
        var record = new FhirSyncRecord();
        record.Direction.Should().Be(FhirSyncDirection.Export);
        record.Status.Should().Be(FhirSyncStatus.Pending);
    }
}
