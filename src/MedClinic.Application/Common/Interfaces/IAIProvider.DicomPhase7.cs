namespace MedClinic.Application.Common.Interfaces;

public partial interface IAIProvider
{
    Task<DicomAIResult> AnalyzeDicomStudyAsync(
        string studyInstanceUid, string modality, CancellationToken ct = default);
}

public record DicomAIResult(
    string Findings,
    List<string> Impressions,
    List<string> Recommendations,
    decimal ConfidenceScore
);
