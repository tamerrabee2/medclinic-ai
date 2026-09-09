using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class ExternalLabsTests
{
    [Fact]
    public void ExternalLabProvider_DefaultShouldBeActive()
    {
        var provider = new ExternalLabProvider();
        provider.IsActive.Should().BeTrue();
    }
}
