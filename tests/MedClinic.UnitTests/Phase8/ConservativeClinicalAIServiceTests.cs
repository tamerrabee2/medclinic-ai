using FluentAssertions;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Infrastructure.ClinicalDecisionSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MedClinic.UnitTests.Phase8;

public class ConservativeClinicalAIServiceTests
{
    private readonly ConservativeClinicalAIService _service = new(NullLogger<ConservativeClinicalAIService>.Instance);
    [Fact] public async Task RiskScore_Hypoxemia_ReturnsCriticalRiskAndReviewRequirement() { var result = await _service.CalculateRiskScoreAsync(new RiskScoreRequest(Guid.NewGuid(), 80, new Dictionary<string, string> { ["SpO2"] = "88" }, null)); result.Band.Should().Be(RiskBand.Critical); result.RequiresClinicianReview.Should().BeTrue(); }
    [Fact] public async Task Triage_ChestPain_ReturnsEmergency() { var result = await _service.AssessTriageAsync(new TriageRequest(Guid.NewGuid(), new[] { "chest pain" }, null)); result.Level.Should().Be(TriageLevel.Emergency); }
    [Fact] public async Task SoapDraft_FreeText_PreservesSourceTextInSubjective() { var result = await _service.SummarizeSoapNoteAsync(new SoapNoteRequest(Guid.NewGuid(), "Patient reports persistent headache for two days.")); result.Subjective.Should().Contain("persistent headache"); result.RequiresClinicianReview.Should().BeTrue(); }
    [Fact] public async Task RadiologyDraft_AlwaysRequiresRadiologistReview() { var result = await _service.GenerateRadiologyReportDraftAsync(new RadiologyReportRequest(Guid.NewGuid(), "CT", "Chest", "No focal consolidation.", new[] { "No acute cardiopulmonary abnormality." })); result.RequiresRadiologistReview.Should().BeTrue(); result.Impression.Should().Contain("No acute"); }
}
