using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class PatientPortalTests
{
    [Fact]
    public void PatientPortalAccess_DefaultShouldBeActive()
    {
        var access = new PatientPortalAccess();
        access.IsActive.Should().BeTrue();
    }

    [Fact]
    public void PatientPortalMessage_DefaultShouldBeUnread()
    {
        var message = new PatientPortalMessage();
        message.IsRead.Should().BeFalse();
    }
}
