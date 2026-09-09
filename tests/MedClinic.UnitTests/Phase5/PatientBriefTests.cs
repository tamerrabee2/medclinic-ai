using FluentAssertions;
using MedClinic.Domain.Entities;
using Xunit;

namespace MedClinic.UnitTests.Phase5;

public class PatientBriefTests
{
    [Fact]
    public void PatientBrief_ShouldRequireDoctorReviewByDefault()
    {
        var brief = new PatientBrief();
        brief.RequiresDoctorReview.Should().BeTrue();
        brief.DoctorReviewed.Should().BeFalse();
    }

    [Fact]
    public void FollowUpIntelligence_DefaultPriority_ShouldBeRoutine()
    {
        var followUp = new FollowUpIntelligence();
        followUp.Priority.Should().Be(FollowUpPriority.Routine);
        followUp.DoctorApproved.Should().BeFalse();
        followUp.DoctorDismissed.Should().BeFalse();
    }

    [Fact]
    public void ClinicalTimelineEvent_DefaultHighlight_ShouldBeFalse()
    {
        var evt = new ClinicalTimelineEvent();
        evt.IsAIHighlighted.Should().BeFalse();
        evt.MetadataJson.Should().Be("{}");
    }
}
