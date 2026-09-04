using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AIConversationConfiguration : IEntityTypeConfiguration<AIConversation>
{
    public void Configure(EntityTypeBuilder<AIConversation> builder)
    {
        builder.ToTable("AIConversations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(300);
        builder.Property(x => x.AIProvider).HasMaxLength(100);
        builder.Property(x => x.ModelVersion).HasMaxLength(100);
        builder.Property(x => x.ContextType).HasMaxLength(100);

        builder.HasIndex(x => new { x.ClinicId, x.DoctorId });
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
