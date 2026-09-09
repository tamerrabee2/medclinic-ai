using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class DentalChartTests
{
    [Fact]
    public void ToothRecord_DefaultStatus_ShouldBeNormal()
    {
        var tooth = new ToothRecord();
        tooth.Status.Should().Be(ToothStatus.Normal);
        tooth.SurfaceDataJson.Should().Be("{}");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(36)]
    [InlineData(48)]
    public void ToothNumbers_ShouldSupportFDINotation(int toothNumber)
    {
        var tooth = new ToothRecord { ToothNumber = toothNumber };
        tooth.ToothNumber.Should().Be(toothNumber);
    }

    [Fact]
    public void DentalChart_ShouldContainToothCollection()
    {
        var chart = new DentalChart();
        chart.Teeth.Should().NotBeNull();
        chart.Teeth.Should().BeEmpty();
    }
}
