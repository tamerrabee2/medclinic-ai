using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class InsuranceTests
{
    [Fact]
    public void InsuranceProvider_DefaultShouldBeActive()
    {
        var provider = new InsuranceProvider();
        provider.IsActive.Should().BeTrue();
    }

    [Fact]
    public void InsuranceClaim_DefaultStatus_ShouldBeDraft()
    {
        var claim = new InsuranceClaim();
        claim.Status.Should().Be(ClaimStatus.Draft);
    }
}
