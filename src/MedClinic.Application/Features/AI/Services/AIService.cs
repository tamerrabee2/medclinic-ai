using IAiConsentGuard = MedClinic.Application.Common.Interfaces.IAiConsentGuard;
using MedClinic.Application.Features.AI.DTOs;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedClinic.Application.Features.AI.Services;

public class AIService
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext       _tenant;
    private readonly IAIProvider          _ai;
    private readonly ILogger<AIService>   _logger;
    private readonly IAiDecisionAuditService? _auditService;
    private readonly IAiConsentGuard?     _consentGuard;

    public AIService(
        IApplicationDbContext db,
        ITenantContext tenant,
        IAIProvider ai,
        ILogger<AIService> logger,
        IAiDecisionAuditService? auditService = null,
        IAiConsentGuard? consentGuard = null)
    {
        _db           = db;
        _tenant       = tenant;
        _ai           = ai;
        _logger       = logger;
        _auditService = auditService;
        _consentGuard = consentGuard;
    }

    // ── Conversations ────────────────────────────────────────────────────────

    public async Task<List<ConversationSummaryDto>> GetConversationsAsync(
        Guid userId, CancellationToken ct = default)
    {
        return await _db.AIConversations
            .Where(c => c.UserId == userId && c.ClinicId == _tenant.ClinicId)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ConversationSummaryDto(
                c.Id,
                c.Title ?? "New Conversation",
                c.Messages.OrderByDescending(m => m.CreatedAt)
                          .Select(m => m.Content)
                          .FirstOrDefault(),
                c.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<ConversationDto> GetConversationAsync(
        Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        var conv = await _db.AIConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c =>
                c.Id == conversationId &&
                c.UserId == userId &&
                c.ClinicId == _tenant.ClinicId, ct)
            ?? throw new KeyNotFoundException("Conversation not found.");

        string? patientName = null;
        if (conv.PatientContextId.HasValue)
        {
            var p = await _db.Patients.FindAsync([conv.PatientContextId.Value], ct);
            patientName = p != null ? $"{p.FirstName} {p.LastName}" : null;
        }

        return new ConversationDto(
            conv.Id, conv.Title ?? "Conversation", conv.PatientContextId, patientName,
            conv.Messages.OrderBy(m => m.CreatedAt).Select(m => new AIMessageDto(
                m.Id, m.Role, m.Content, false, m.CreatedAt)).ToList(),
            conv.CreatedAt, conv.UpdatedAt);
    }

    public async Task<ConversationDto> SendMessageAsync(
        Guid userId, SendMessageRequest req, CancellationToken ct = default)
    {
        if (req.PatientContextId.HasValue && _consentGuard != null)
        {
            await _consentGuard.EnsureAiConsentAsync(req.PatientContextId.Value, ct);
        }

        // 1. Resolve or create conversation
        AIConversation conversation;
        if (req.ConversationId.HasValue)
        {
            conversation = await _db.AIConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.Id == req.ConversationId.Value &&
                    c.UserId == userId &&
                    c.ClinicId == _tenant.ClinicId, ct)
                ?? throw new KeyNotFoundException("Conversation not found.");
        }
        else
        {
            conversation = new AIConversation
            {
                Id               = Guid.NewGuid(),
                UserId           = userId,
                ClinicId         = _tenant.ClinicId ?? Guid.Empty,
                Title            = TruncateTitle(req.Message),
                PatientContextId = req.PatientContextId,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow,
                Messages         = []
            };
            _db.AIConversations.Add(conversation);
        }

        // 2. Build history + patient context
        var systemPrompt = await BuildSystemPromptAsync(req.PatientContextId, ct);
        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToList();

        // 3. Save user message
        var userMsg = new AIConversationMessage
        {
            Id             = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role           = "user",
            Content        = req.Message,
            CreatedAt      = DateTime.UtcNow,
            SentAt         = DateTime.UtcNow
        };
        conversation.Messages.Add(userMsg);
        _db.AIConversationMessages.Add(userMsg);

        // 4. Call AI provider
        var aiRequest = new AIChatRequest(
            SystemPrompt: systemPrompt,
            History: history,
            UserMessage: req.Message,
            AttachmentBase64: req.AttachmentBase64,
            AttachmentMimeType: req.AttachmentMimeType
        );

        AIChatResponse aiResponse;
        try
        {
            aiResponse = await _ai.ChatAsync(aiRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI provider error for user {UserId}", userId);
            aiResponse = new AIChatResponse(
                "I'm sorry, I encountered an issue processing your request. " +
                "Please try again or contact support if the problem persists.",
                false);
        }

        // 5. Save assistant message
        var assistantMsg = new AIConversationMessage
        {
            Id             = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role           = "assistant",
            Content        = aiResponse.Content,
            CreatedAt      = DateTime.UtcNow,
            SentAt         = DateTime.UtcNow
        };
        conversation.Messages.Add(assistantMsg);
        _db.AIConversationMessages.Add(assistantMsg);
        conversation.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        if (_auditService != null)
        {
            await _auditService.RecordDecisionAsync(new RecordAiDecisionRequest
            {
                Capability = "ClinicalChat",
                ProviderName = _ai.GetType().Name,
                PatientId = conversation.PatientContextId,
                DoctorId = userId,
                InputPayload = req.Message,
                OutputPayload = aiResponse.Content,
                CorrelationId = conversation.Id.ToString()
            }, ct);
        }

        string? patientName = null;
        if (conversation.PatientContextId.HasValue)
        {
            var p = await _db.Patients.FindAsync([conversation.PatientContextId.Value], ct);
            patientName = p != null ? $"{p.FirstName} {p.LastName}" : null;
        }

        return new ConversationDto(
            conversation.Id, conversation.Title ?? "Conversation",
            conversation.PatientContextId, patientName,
            conversation.Messages.OrderBy(m => m.CreatedAt).Select(m => new AIMessageDto(
                m.Id, m.Role, m.Content, false, m.CreatedAt)).ToList(),
            conversation.CreatedAt, conversation.UpdatedAt);
    }

    public async Task DeleteConversationAsync(
        Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        var conv = await _db.AIConversations
            .FirstOrDefaultAsync(c =>
                c.Id == conversationId &&
                c.UserId == userId &&
                c.ClinicId == _tenant.ClinicId, ct)
            ?? throw new KeyNotFoundException("Conversation not found.");

        _db.AIConversations.Remove(conv);
        await _db.SaveChangesAsync(ct);
    }

    // ── Lab Analysis ─────────────────────────────────────────────────────────

    public async Task<LabAnalysisResultDto> AnalyzeLabResultAsync(
        Guid userId, AnalyzeLabRequest req, CancellationToken ct = default)
    {
        var labResult = await _db.LabResults
            .Include(r => r.Items)
            .Include(r => r.LabOrder)
            .FirstOrDefaultAsync(r =>
                r.Id == req.LabResultId &&
                r.LabOrder.ClinicId == _tenant.ClinicId, ct)
            ?? throw new KeyNotFoundException("Lab result not found.");

        if (_consentGuard != null)
        {
            await _consentGuard.EnsureAiConsentAsync(labResult.LabOrder.PatientId, ct);
        }

        // Build previous results for comparison
        List<LabResult>? previousResults = null;
        if (req.CompareWithPrevious)
        {
            previousResults = await _db.LabResults
                .Include(r => r.Items)
                .Include(r => r.LabOrder)
                .Where(r =>
                    r.LabOrder.PatientId == labResult.LabOrder.PatientId &&
                    r.Id != req.LabResultId &&
                    r.ReportedAt < labResult.ReportedAt)
                .OrderByDescending(r => r.ReportedAt)
                .Take(3)
                .ToListAsync(ct);
        }

        var input = new LabAnalysisInput(
            CurrentResult: labResult,
            PreviousResults: previousResults ?? []
        );

        var result = await _ai.AnalyzeLabResultsAsync(input, ct);

        if (_auditService != null)
        {
            await _auditService.RecordDecisionAsync(new RecordAiDecisionRequest
            {
                Capability = "LabAnalysis",
                ProviderName = _ai.GetType().Name,
                PatientId = labResult.LabOrder.PatientId,
                DoctorId = userId,
                InputPayload = $"LabResultId: {labResult.Id}, Items: {labResult.Items.Count}",
                OutputPayload = result.Summary,
                CorrelationId = labResult.Id.ToString()
            }, ct);
        }

        return new LabAnalysisResultDto(
            labResult.Id,
            result.Summary,
            result.Values.Select(v => new LabValueAnalysis(
                v.TestName, v.CurrentValue, v.PreviousValue,
                v.ReferenceRange, v.Status, v.Trend)).ToList(),
            result.Abnormalities,
            result.Trends,
            result.Recommendations,
            RequiresDoctorReview: true,
            Disclaimer: AIDisclaimerText
        );
    }

    // ── Patient Summary ───────────────────────────────────────────────────────

    public async Task<PatientSummaryDto> GeneratePatientSummaryAsync(
        Guid userId, GeneratePatientSummaryRequest req, CancellationToken ct = default)
    {
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p =>
                p.Id == req.PatientId &&
                p.ClinicId == _tenant.ClinicId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (_consentGuard != null)
        {
            await _consentGuard.EnsureAiConsentAsync(patient.Id, ct);
        }

        var recentVisits = await _db.Visits
            .Where(v => v.PatientId == req.PatientId)
            .OrderByDescending(v => v.VisitDate)
            .Take(5)
            .ToListAsync(ct);

        var input = new PatientSummaryInput(
            Patient: patient,
            RecentVisits: recentVisits,
            IncludeLabTrends: req.IncludeLabTrends,
            IncludeRadiology: req.IncludeRadiology,
            IncludeMedications: req.IncludeMedications
        );

        var result = await _ai.SummarizePatientAsync(input, ct);

        if (_auditService != null)
        {
            await _auditService.RecordDecisionAsync(new RecordAiDecisionRequest
            {
                Capability = "PatientSummary",
                ProviderName = _ai.GetType().Name,
                PatientId = patient.Id,
                DoctorId = userId,
                InputPayload = $"PatientId: {patient.Id}, LabTrends: {req.IncludeLabTrends}, Radiology: {req.IncludeRadiology}",
                OutputPayload = result.Summary,
                CorrelationId = patient.Id.ToString()
            }, ct);
        }

        return new PatientSummaryDto(
            patient.Id,
            $"{patient.FirstName} {patient.LastName}",
            result.Summary,
            result.MedicalHistoryHighlights,
            result.ActiveConditions,
            result.CurrentMedications,
            result.RecentAbnormalities,
            result.UpcomingFollowUps,
            RequiresDoctorReview: true,
            Disclaimer: AIDisclaimerText,
            GeneratedAt: DateTime.UtcNow
        );
    }

    // ── Medical Image Analysis ────────────────────────────────────────────────

    public async Task<ImageAnalysisResultDto> AnalyzeImageAsync(
        Guid userId, AnalyzeImageRequest req, CancellationToken ct = default)
    {
        var image = await _db.MedicalImages
            .Include(i => i.RadiologyStudy)
            .FirstOrDefaultAsync(i =>
                i.Id == req.RadiologyImageId &&
                i.RadiologyStudy.ClinicId == _tenant.ClinicId, ct)
            ?? throw new KeyNotFoundException("Medical image not found.");

        if (_consentGuard != null)
        {
            await _consentGuard.EnsureAiConsentAsync(image.RadiologyStudy.PatientId, ct);
        }

        var input = new MedicalImageInput(
            ImageId: image.Id,
            ImagePath: image.FileUrl,
            Modality: image.Modality ?? image.RadiologyStudy.StudyType,
            ClinicalContext: req.ClinicalContext
        );

        var result = await _ai.AnalyzeMedicalImageAsync(input, ct);

        // Update image record with analysis status
        image.AIAnalysisStatus = "Completed";
        image.AIAnalyzedAt     = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if (_auditService != null)
        {
            await _auditService.RecordDecisionAsync(new RecordAiDecisionRequest
            {
                Capability = "ImageAnalysis",
                ProviderName = _ai.GetType().Name,
                PatientId = image.RadiologyStudy.PatientId,
                DoctorId = userId,
                InputPayload = $"ImageId: {image.Id}, Modality: {input.Modality}, Context: {req.ClinicalContext}",
                OutputPayload = result.Findings != null ? string.Join("; ", result.Findings) : (result.Summary ?? string.Empty),
                ConfidenceScore = (double?)result.Confidence,
                CorrelationId = image.Id.ToString()
            }, ct);
        }

        return new ImageAnalysisResultDto(
            image.Id,
            result.Summary,
            result.Findings,
            result.Observations,
            result.RegionsOfInterest.Select(r => new AIRegionOfInterest(
                r.Description, r.X, r.Y, r.Width, r.Height)).ToList(),
            result.Confidence,
            result.RecommendationsForReview,
            RequiresDoctorReview: true,
            Disclaimer: AIDisclaimerText,
            GeneratedAt: DateTime.UtcNow
        );
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> BuildSystemPromptAsync(
        Guid? patientId, CancellationToken ct)
    {
        const string basePrompt =
            "You are Dr. AI, a clinical decision support assistant for healthcare professionals. " +
            "You provide evidence-based information to assist doctors in their clinical decisions. " +
            "You do NOT make final diagnoses, prescribe medications, or replace physician judgment. " +
            "Always recommend doctor review for any clinical findings. " +
            "Be concise, structured, and use medical terminology appropriately.";

        if (!patientId.HasValue)
            return basePrompt;

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == patientId.Value, ct);

        if (patient == null) return basePrompt;

        var context = $"\n\nCurrent patient context:\n" +
            $"- Name: {patient.FirstName} {patient.LastName}\n" +
            $"- Age: {CalculateAge(patient.DateOfBirth)} years\n" +
            $"- Gender: {patient.Gender}\n" +
            $"- Allergies: {(!string.IsNullOrWhiteSpace(patient.Allergies) ? patient.Allergies : "None recorded")}";

        return basePrompt + context;
    }

    private static int CalculateAge(DateTime? dob)
    {
        if (!dob.HasValue) return 0;
        var today = DateTime.Today;
        var age = today.Year - dob.Value.Year;
        if (dob.Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static string TruncateTitle(string message)
    {
        var title = message.Length > 60 ? message[..57] + "..." : message;
        return title.Replace("\n", " ").Trim();
    }

    private const string AIDisclaimerText =
        "⚠️ AI-generated content is for clinical decision support only and must be reviewed " +
        "by a qualified healthcare professional before any clinical action is taken.";
}
