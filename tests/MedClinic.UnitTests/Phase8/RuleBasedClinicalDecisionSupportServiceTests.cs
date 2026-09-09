using FluentAssertions;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Infrastructure.ClinicalDecisionSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MedClinic.UnitTests.Phase8;

public class RuleBasedClinicalDecisionSupportServiceTests
{
    private readonly RuleBasedClinicalDecisionSupportService _service = new(NullLogger<RuleBasedClinicalDecisionSupportService>.Instance);

    [Fact]
    public async Task GenerateDifferential_ChestPain_ReturnsCandidateAndCriticalAlert()
    {
        var result = await _service.GenerateDifferentialAsync(new DifferentialDiagnosisRequest(Guid.NewGuid(), new[] { "chest pain" }, null, null, null));
        result.RequiresClinicianReview.Should().BeTrue();
        result.Candidates.Should().Contain(x => x.Condition == "Acute coronary syndrome");
        result.Alerts.Should().Contain(x => x.Severity == AlertSeverity.Critical);
    }

    [Fact]
    public async Task CheckDrugInteractions_WarfarinAndIbuprofen_ReturnsMajorAlert()
    {
        var result = await _service.CheckDrugInteractionsAsync(new DrugInteractionRequest(Guid.NewGuid(), new[] { new MedicationInput("Warfarin"), new MedicationInput("Ibuprofen") }));
        result.RequiresClinicianReview.Should().BeTrue();
        result.Interactions.Should().ContainSingle(x => x.Severity == InteractionSeverity.Major);
    }

    [Fact]
    public async Task CheckDrugInteractions_NitrateAndSildenafil_ReturnsContraindicatedAlert()
    {
        var result = await _service.CheckDrugInteractionsAsync(new DrugInteractionRequest(Guid.NewGuid(), new[] { new MedicationInput("Nitroglycerin"), new MedicationInput("Sildenafil") }));
        result.Interactions.Should().ContainSingle(x => x.Severity == InteractionSeverity.Contraindicated);
    }
}
