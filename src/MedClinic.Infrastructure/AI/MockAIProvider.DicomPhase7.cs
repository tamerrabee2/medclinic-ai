using MedClinic.Application.Common.Interfaces;

namespace MedClinic.Infrastructure.AI;

public partial class MockAIProvider
{
    public Task<DicomAIResult> AnalyzeDicomStudyAsync(
        string studyInstanceUid, string modality, CancellationToken ct = default)
    {
        var (findings, impressions, recommendations) = modality.ToUpperInvariant() switch
        {
            "CT" => (
                "Axial CT sections reviewed. Lung windows show no consolidation or pneumothorax. Mediastinum is midline. No significant lymphadenopathy.",
                new List<string> { "No acute cardiopulmonary pathology identified.", "Mild emphysematous changes bilateral upper lobes." },
                new List<string> { "Clinical correlation recommended.", "Follow-up imaging if symptoms persist." }
            ),
            "CR" or "DX" => (
                "Plain radiograph reviewed. Bones show normal mineralization. No fracture line identified. Joint spaces preserved.",
                new List<string> { "No acute osseous abnormality.", "Soft tissue swelling noted laterally." },
                new List<string> { "Weight-bearing views if clinically indicated.", "MRI for soft tissue evaluation if pain persists." }
            ),
            "MR" => (
                "MRI sequences reviewed. Signal intensity within normal limits for age. No focal lesion. Disc heights maintained.",
                new List<string> { "No significant structural abnormality identified.", "Age-appropriate degenerative changes noted." },
                new List<string> { "Correlate with clinical symptoms.", "EMG/NCS if radiculopathy suspected." }
            ),
            "US" => (
                "Ultrasound images reviewed. No free fluid. Parenchymal echogenicity within normal limits. No focal mass lesion.",
                new List<string> { "No sonographic abnormality detected." },
                new List<string> { "Repeat ultrasound in 3 months if symptoms persist." }
            ),
            _ => (
                "Medical imaging study reviewed.",
                new List<string> { "No acute findings identified." },
                new List<string> { "Clinical correlation recommended." }
            )
        };

        return Task.FromResult(new DicomAIResult(findings, impressions, recommendations, 0.82m));
    }
}
