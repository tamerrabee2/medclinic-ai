using MedClinic.Domain.Entities;

namespace MedClinic.Application.Common.Interfaces;

/// <summary>Phase 5 AI capabilities — Voice Scribe, Patient Brief, Timeline, Follow-up.</summary>
public partial interface IAIProvider
{
    // Voice Scribe
    Task<VoiceTranscriptionResult> TranscribeVoiceNoteAsync(string audioFileUrl, CancellationToken ct = default);
    Task<StructuredClinicalNote> ParseClinicalNoteFromTranscriptAsync(string transcript, CancellationToken ct = default);

    // AI Patient Brief
    Task<AIPatientBriefResult> GeneratePatientBriefAsync(Patient patient, IEnumerable<LabResult> recentLabs, CancellationToken ct = default);

    // AI Timeline
    Task<AITimelineSummaryResult> SummarizePatientTimelineAsync(IEnumerable<ClinicalTimelineEvent> events, CancellationToken ct = default);

    // Follow-up Intelligence
    Task<List<AIFollowUpSuggestion>> GenerateFollowUpSuggestionsAsync(Patient patient, CancellationToken ct = default);
}

// Result types
public record VoiceTranscriptionResult(
    string RawTranscript,
    string Language,
    decimal Confidence
);

public record StructuredClinicalNote(
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? PhysicalExamination,
    string? Assessment,
    string? Plan
);

public record AIPatientBriefResult(
    string Summary,
    List<string> RecentChanges,
    List<string> PendingItems,
    List<string> Alerts,
    string ModelUsed,
    decimal ConfidenceScore
);

public record AITimelineSummaryResult(
    string NarrativeSummary,
    List<string> KeyChanges,
    List<string> CriticalEvents,
    string TrendAssessment,
    List<Guid>? HighlightedEventIds
);

public record AIFollowUpSuggestion(
    string Reason,
    string RecommendedAction,
    FollowUpPriority Priority,
    int SuggestedDaysFromNow,
    List<string> Triggers
);
