using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIConversation : TenantEntity
{
    public Guid? DoctorId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? PatientContextId { get => PatientId; set => PatientId = value; }
    public string? Title { get; set; }
    public string? AIProvider { get; set; }
    public string? ModelVersion { get; set; }
    public string? ContextType { get; set; } // General, PatientSummary, LabAnalysis, etc.
    public bool IsArchived { get; set; } = false;
    public int MessageCount { get; set; } = 0;

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
    public ICollection<AIConversationMessage> Messages { get; set; } = [];
}
