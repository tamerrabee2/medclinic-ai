using FluentAssertions;
using MedClinic.Application.Common.Models;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class Phase7StabilizationTests
{
    [Fact]
    public void Result_Success_ShouldHaveSucceededTrue()
    {
        var result = Result.Success();
        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Result_Failure_ShouldHaveSucceededFalse()
    {
        var result = Result.Failure("Something went wrong");
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void ResultT_Success_ShouldContainData()
    {
        var result = Result<string>.Success("hello");
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be("hello");
    }

    [Fact]
    public void PaginatedList_ShouldComputePages()
    {
        var list = new PaginatedList<int>(new List<int> { 1, 2, 3 }, totalCount: 30, page: 2, pageSize: 10);
        list.TotalPages.Should().Be(3);
        list.HasPreviousPage.Should().BeTrue();
        list.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void ValidationException_ShouldContainErrors()
    {
        var ex = new ValidationException(new[] { "Field required", "Invalid format" });
        ex.Errors.Should().HaveCount(2);
    }
}
