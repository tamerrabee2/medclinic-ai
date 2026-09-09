using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase7;

public class DicomStudyTests
{
    [Fact]
    public void DicomStudy_DefaultStatus_ShouldBePending()
    {
        var study = new DicomStudy();
        study.Status.Should().Be(DicomStudyStatus.Pending);
        study.HasAIAnalysis.Should().BeFalse();
        study.AIReviewedByDoctor.Should().BeFalse();
    }

    [Fact]
    public void DicomStudy_AfterAIAnalysis_ShouldRequireReview()
    {
        var study = new DicomStudy
        {
            HasAIAnalysis = true,
            AIFindings = "No acute findings.",
            AIReviewedByDoctor = false
        };
        study.HasAIAnalysis.Should().BeTrue();
        study.AIReviewedByDoctor.Should().BeFalse();
    }

    [Fact]
    public void DicomStudy_AfterDoctorApproval_AIReviewedShouldBeTrue()
    {
        var study = new DicomStudy
        {
            HasAIAnalysis = true,
            AIReviewedByDoctor = true
        };
        study.AIReviewedByDoctor.Should().BeTrue();
    }

    [Theory]
    [InlineData("CT")]
    [InlineData("MR")]
    [InlineData("CR")]
    [InlineData("US")]
    [InlineData("DX")]
    public void DicomStudy_Modality_ShouldSupportCommonTypes(string modality)
    {
        var study = new DicomStudy { Modality = modality };
        study.Modality.Should().Be(modality);
    }

    [Fact]
    public void DicomSeries_ShouldBelongToStudy()
    {
        var studyId = Guid.NewGuid();
        var series = new DicomSeries { DicomStudyId = studyId };
        series.DicomStudyId.Should().Be(studyId);
        series.Instances.Should().BeEmpty();
    }
}
