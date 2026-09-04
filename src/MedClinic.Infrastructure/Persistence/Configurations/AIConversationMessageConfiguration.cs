using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AIConversationMessageConfiguration : IEntityTypeConfiguration<AIConversationMessage>
{
    public void Configure(EntityTypeBuilder<AIConversationMessage> builder)
    {
        builder.ToTable("AIConversationMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.AttachmentUrl).HasMaxLength(500);
        builder.Property(x => x.AttachmentType).HasMaxLength(100);

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.SentAt);
    }
}
