using MedClinic.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/patients/{patientId:guid}/clinical-decision-support")]
public sealed class ClinicalDecisionSupportController : ControllerBase
{
    private readonly IClinicalDecisionSupportService _clinicalDecisionSupport;

    public ClinicalDecisionSupportController(IClinicalDecisionSupportService clinicalDecisionSupport) => _clinicalDecisionSupport = clinicalDecisionSupport;

    [HttpPost("differential-diagnosis")]
    [ProducesResponseType(typeof(DifferentialDiagnosisResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DifferentialDiagnosisResult>> GenerateDifferential(Guid patientId, [FromBody] DifferentialDiagnosisApiRequest request, CancellationToken ct)
    {
        if (request.Symptoms is null || request.Symptoms.Count == 0)
            return ValidationProblem(new Dictionary<string, string[]> { ["symptoms"] = new[] { "At least one symptom is required." } });
        var result = await _clinicalDecisionSupport.GenerateDifferentialAsync(new DifferentialDiagnosisRequest(patientId, request.Symptoms, request.Vitals, request.LabResults, request.ClinicalNotes), ct);
        return Ok(result);
    }

    [HttpPost("drug-interactions")]
    [ProducesResponseType(typeof(DrugInteractionResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DrugInteractionResult>> CheckDrugInteractions(Guid patientId, [FromBody] DrugInteractionApiRequest request, CancellationToken ct)
    {
        if (request.Medications is null || request.Medications.Count < 2)
            return ValidationProblem(new Dictionary<string, string[]> { ["medications"] = new[] { "At least two medications are required for an interaction check." } });
        var result = await _clinicalDecisionSupport.CheckDrugInteractionsAsync(new DrugInteractionRequest(patientId, request.Medications.Select(x => new MedicationInput(x.Name, x.Dose, x.Route)).ToList()), ct);
        return Ok(result);
    }
}

public sealed record DifferentialDiagnosisApiRequest(List<string> Symptoms, Dictionary<string, string>? Vitals, Dictionary<string, string>? LabResults, string? ClinicalNotes);
public sealed record DrugInteractionApiRequest(List<MedicationApiInput> Medications);
public sealed record MedicationApiInput(string Name, string? Dose, string? Route);
