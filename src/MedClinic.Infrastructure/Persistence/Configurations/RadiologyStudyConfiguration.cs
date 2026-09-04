using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class RadiologyStudyConfiguration : IEntityTypeConfiguration<RadiologyStudy>
{
    public void Configure(EntityTypeBuilder<RadiologyStudy> builder)
    {
        builder.ToTable("RadiologyStudies");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudyType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BodyPart).HasMaxLength(100);
        builder.Property(x => x.ClinicalInfo).HasMaxLength(2000);
        builder.Property(x => x.Findings).HasMaxLength(4000);
        builder.Property(x => x.Impression).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ReportedBy).HasMaxLength(200);
        builder.Property(x => x.AccessionNumber).HasMaxLength(100);

        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
        builder.HasIndex(x => x.StudyDate);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.RadiologyStudies)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.RadiologyStudy)
            .HasForeignKey(x => x.RadiologyStudyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
