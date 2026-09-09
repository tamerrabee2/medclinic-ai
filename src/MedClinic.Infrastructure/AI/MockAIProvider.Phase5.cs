using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;

namespace MedClinic.Infrastructure.AI;

/// <summary>Mock AI Phase 5 — zero-cost demo implementation.</summary>
public partial class MockAIProvider : IAIProvider
{
    public Task<VoiceTranscriptionResult> TranscribeVoiceNoteAsync(string audioFileUrl, CancellationToken ct = default)
    {
        return Task.FromResult(new VoiceTranscriptionResult(
            RawTranscript: "Patient complains of knee pain for two weeks, worsening with movement. No fever. PMH: hypertension.",
            Language: "en",
            Confidence: 0.94m
        ));
    }

    public Task<StructuredClinicalNote> ParseClinicalNoteFromTranscriptAsync(string transcript, CancellationToken ct = default)
    {
        return Task.FromResult(new StructuredClinicalNote(
            ChiefComplaint: "Right knee pain for 2 weeks",
            HistoryOfPresentIllness: "Patient reports progressive right knee pain worsening with movement and weight bearing. No trauma. No fever or swelling noted.",
            PhysicalExamination: "Right knee: mild tenderness on palpation. Range of motion mildly restricted. No effusion.",
            Assessment: "Mechanical knee pain, likely degenerative. Rule out meniscal pathology.",
            Plan: "NSAID for 1 week. Physiotherapy referral. X-ray right knee AP/lateral. Follow up in 2 weeks."
        ));
    }

    public Task<AIPatientBriefResult> GeneratePatientBriefAsync(Patient patient, IEnumerable<LabResult> recentLabs, CancellationToken ct = default)
    {
        return Task.FromResult(new AIPatientBriefResult(
            Summary: $"{patient.FirstName} {patient.LastName} is a {patient.Age}-year-old patient with ongoing management. Review recent results before consultation.",
            RecentChanges: new List<string> { "HbA1c improved from 8.2 to 7.4", "Blood pressure trending down" },
            PendingItems: new List<string> { "Follow-up lab due", "Radiology review pending" },
            Alerts: new List<string> { "Patient missed last appointment" },
            ModelUsed: "MockAIProvider",
            ConfidenceScore: 0.85m
        ));
    }

    public Task<AITimelineSummaryResult> SummarizePatientTimelineAsync(IEnumerable<ClinicalTimelineEvent> events, CancellationToken ct = default)
    {
        var eventList = events.ToList();
        return Task.FromResult(new AITimelineSummaryResult(
            NarrativeSummary: $"Patient has {eventList.Count} recorded clinical events. Overall trajectory shows gradual improvement in metabolic parameters.",
            KeyChanges: new List<string> { "Medication adjustment 3 months ago", "Lab values improving" },
            CriticalEvents: new List<string> { "ER visit 6 months ago" },
            TrendAssessment: "Stable with positive trend",
            HighlightedEventIds: eventList.Take(2).Select(e => e.Id).ToList()
        ));
    }

    public Task<List<AIFollowUpSuggestion>> GenerateFollowUpSuggestionsAsync(Patient patient, CancellationToken ct = default)
    {
        return Task.FromResult(new List<AIFollowUpSuggestion>
        {
            new("HbA1c monitoring required", "Repeat HbA1c lab test", FollowUpPriority.High, 90, new List<string> { "Diabetes management", "Last HbA1c > 7.0" }),
            new("Blood pressure follow-up", "BP check and medication review", FollowUpPriority.Routine, 30, new List<string> { "Hypertension", "New medication started" })
        });
    }
}
