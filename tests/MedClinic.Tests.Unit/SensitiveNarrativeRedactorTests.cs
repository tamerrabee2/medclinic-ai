using FluentAssertions;
using MedClinic.Shared.Common;
using Xunit;

namespace MedClinic.Tests.Unit;

public class SensitiveNarrativeRedactorTests
{
    [Fact]
    public void Redact_WhenMaskPiiIsFalse_ReturnsOriginalNarrative()
    {
        // Arrange
        const string input = "Patient requested revocation due to personal reasons.";

        // Act
        var result = SensitiveNarrativeRedactor.Redact(input, maskPii: false);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void Redact_WhenMaskPiiIsTrue_ReturnsRedactedPlaceholder()
    {
        // Arrange
        const string input = "Patient with MRN 12345 experienced allergic reaction.";

        // Act
        var result = SensitiveNarrativeRedactor.Redact(input, maskPii: true);

        // Assert
        result.Should().Be("[REDACTED — requires privileged export permission]");
    }

    [Fact]
    public void Redact_WhenMaskPiiIsTrue_WithCustomPlaceholder_ReturnsCustomPlaceholder()
    {
        // Arrange
        const string input = "Clinician rejected diagnosis due to conflicting lab work.";
        const string customPlaceholder = "[REDACTED — requires privileged view permission]";

        // Act
        var result = SensitiveNarrativeRedactor.Redact(input, maskPii: true, customPlaceholder);

        // Assert
        result.Should().Be(customPlaceholder);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Redact_WhenInputIsNullOrWhitespace_ReturnsEmptyString(string? input)
    {
        // Act & Assert
        SensitiveNarrativeRedactor.Redact(input, maskPii: false).Should().Be(string.Empty);
        SensitiveNarrativeRedactor.Redact(input, maskPii: true).Should().Be(string.Empty);
    }
}
