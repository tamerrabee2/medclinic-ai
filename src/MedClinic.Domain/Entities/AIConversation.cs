using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIConversation : TenantEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }
    public string Title { get; set; } = "New Conversation";
    public string Provider { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public Clinic Clinic { get; set; } = null!;

    public ICollection<AIConversationMessage> Messages { get; set; } = [];
}
