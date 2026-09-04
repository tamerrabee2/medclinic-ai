using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class AIAnalysisConfiguration : IEntityTypeConfiguration<AIAnalysis>
{
    public void Configure(EntityTypeBuilder<AIAnalysis> builder)
    {
        builder.ToTable("AIAnalyses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AnalysisType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Summary).HasMaxLength(4000);
        builder.Property(x => x.ResultJson).HasColumnType("jsonb");
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.AIProvider).HasMaxLength(100);
        builder.Property(x => x.ModelVersion).HasMaxLength(100);
        builder.Property(x => x.Disclaimer).HasMaxLength(1000);

        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.AIAnalyses)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByDoctor)
            .WithMany()
            .HasForeignKey(x => x.RequestedByDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByDoctor)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByDoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
