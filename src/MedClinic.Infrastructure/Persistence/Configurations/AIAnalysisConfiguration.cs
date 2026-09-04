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

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Provider)
            .HasMaxLength(100);

        builder.Property(a => a.Summary)
            .HasMaxLength(4000);

        builder.Property(a => a.ResultJson)
            .HasColumnType("jsonb");

        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(a => new { a.ClinicId, a.Status });
        builder.HasIndex(a => a.ReferenceId);
    }
}
