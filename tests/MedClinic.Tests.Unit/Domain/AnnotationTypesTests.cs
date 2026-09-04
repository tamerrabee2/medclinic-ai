using FluentAssertions;
using MedClinic.Shared.Constants;
using Xunit;

namespace MedClinic.Tests.Unit.Domain;

public class AnnotationTypesTests
{
    [Fact]
    public void AnnotationTypes_All_ShouldContainExpectedTypes()
    {
        AnnotationTypes.All.Should().Contain(["Pen", "Arrow", "Rectangle",
            "Circle", "Text", "Measurement"]);
    }

    [Fact]
    public void AnnotationTypes_All_ShouldHaveNoDuplicates()
    {
        AnnotationTypes.All.Should().OnlyHaveUniqueItems();
    }
}
