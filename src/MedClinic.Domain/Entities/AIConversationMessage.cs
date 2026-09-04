using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIConversationMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public AIConversation Conversation { get; set; } = null!;
    public string Role { get; set; } = string.Empty; // user | assistant | system
    public string Content { get; set; } = string.Empty;
    public string? AttachmentsJson { get; set; }
    public int? TokensUsed { get; set; }
}
