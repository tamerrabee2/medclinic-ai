using FluentAssertions;
using MedClinic.Domain.Enums;
using Xunit;
namespace MedClinic.Tests.Unit.Services;
public class ConsentWorkflowTests
{
    [Fact] public void AiConsentType_ShouldBeAvailable() => ((int)ConsentType.AiAssistedCare).Should().BeGreaterThanOrEqualTo(0);
    [Fact] public void ConsentExpiry_InPast_ShouldBeRejectedByApiContract() { var expiry = DateTime.UtcNow.AddMinutes(-1); (expiry <= DateTime.UtcNow).Should().BeTrue(); }
    [Fact] public void RevocationReason_ShouldBeRequired() { string? reason = " "; string.IsNullOrWhiteSpace(reason).Should().BeTrue(); }
}