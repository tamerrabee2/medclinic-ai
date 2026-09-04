using FluentAssertions;
using MedClinic.Shared.Constants;
using Xunit;

namespace MedClinic.Tests.Unit.Domain;

public class BodyRegionsTests
{
    [Fact]
    public void BodyRegions_All_ShouldContain26Regions()
    {
        BodyRegions.All.Should().HaveCount(26);
    }

    [Theory]
    [InlineData("Head")]
    [InlineData("Chest")]
    [InlineData("Abdomen")]
    [InlineData("LeftArm")]
    [InlineData("RightLeg")]
    [InlineData("UpperBack")]
    public void BodyRegions_All_ShouldContainKeyRegions(string region)
    {
        BodyRegions.All.Should().Contain(region);
    }

    [Fact]
    public void BodyRegions_All_ShouldHaveNoDuplicates()
    {
        BodyRegions.All.Should().OnlyHaveUniqueItems();
    }
}
