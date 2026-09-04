using MedClinic.Domain.Common;

namespace MedClinic.Domain.Entities;

public class AIConversationMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // user, assistant, system
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public int? TokensUsed { get; set; }
    public bool IsError { get; set; } = false;

    public AIConversation Conversation { get; set; } = null!;
}
