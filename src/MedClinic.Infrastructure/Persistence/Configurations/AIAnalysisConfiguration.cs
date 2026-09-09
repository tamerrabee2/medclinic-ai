using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AIAnalysisConfiguration : IEntityTypeConfiguration<AIAnalysis>
{
    public void Configure(EntityTypeBuilder<AIAnalysis> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AnalysisType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Findings)
            .HasMaxLength(4000);

        builder.Property(a => a.Confidence)
            .HasPrecision(5, 4);

        builder.Property(a => a.ModelVersion)
            .HasMaxLength(100);

        builder.Property(a => a.ReviewedBy)
            .HasMaxLength(200);

        builder.HasOne(a => a.RadiologyStudy)
            .WithMany(s => s.AIAnalyses)
            .HasForeignKey(a => a.RadiologyStudyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
