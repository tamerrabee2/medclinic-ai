using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AiDecisionAuditConfiguration : IEntityTypeConfiguration<AiDecisionAudit>
{
    public void Configure(EntityTypeBuilder<AiDecisionAudit> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Capability)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ModelVersion)
            .HasMaxLength(100);

        builder.Property(a => a.InputHash)
            .HasMaxLength(128);

        builder.Property(a => a.OutputHash)
            .HasMaxLength(128);

        builder.Property(a => a.OverrideReason)
            .HasMaxLength(2000);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.HasIndex(a => new { a.ClinicId, a.Capability });
        builder.HasIndex(a => a.CorrelationId);
        builder.HasIndex(a => a.ReviewStatus);

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Visit)
            .WithMany()
            .HasForeignKey(a => a.VisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ReviewedByUser)
            .WithMany()
            .HasForeignKey(a => a.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
