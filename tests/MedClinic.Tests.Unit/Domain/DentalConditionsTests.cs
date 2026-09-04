using FluentAssertions;
using MedClinic.Shared.Constants;
using Xunit;

namespace MedClinic.Tests.Unit.Domain;

public class DentalConditionsTests
{
    [Fact]
    public void DentalConditions_All_ShouldContain11Conditions()
    {
        DentalConditions.All.Should().HaveCount(11);
    }

    [Fact]
    public void DentalConditions_Colors_ShouldHaveEntryForEveryCondition()
    {
        foreach (var condition in DentalConditions.All)
            DentalConditions.Colors.Should().ContainKey(condition,
                $"because '{condition}' needs a UI color");
    }

    [Theory]
    [InlineData("Healthy",   "#22C55E")]
    [InlineData("Cavity",    "#EF4444")]
    [InlineData("Missing",   "#6B7280")]
    [InlineData("Implant",   "#8B5CF6")]
    public void DentalConditions_Colors_MatchExpectedHexValues(
        string condition, string expectedColor)
    {
        DentalConditions.Colors[condition].Should().Be(expectedColor);
    }
}
