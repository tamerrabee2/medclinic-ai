using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase6;

public class NotificationTests
{
    [Fact]
    public void NotificationLog_DefaultStatus_ShouldBePending()
    {
        var log = new NotificationLog();
        log.DeliveryStatus.Should().Be(NotificationDeliveryStatus.Pending);
    }

    [Fact]
    public void ClinicReport_DefaultStatus_ShouldBePending()
    {
        var report = new ClinicReport();
        report.Status.Should().Be(ReportStatus.Pending);
    }

    [Theory]
    [InlineData(NotificationChannel.WhatsApp)]
    [InlineData(NotificationChannel.Email)]
    [InlineData(NotificationChannel.InApp)]
    [InlineData(NotificationChannel.SMS)]
    public void NotificationChannel_AllValues_ShouldBeValid(NotificationChannel channel)
    {
        Enum.IsDefined(channel).Should().BeTrue();
    }
}
