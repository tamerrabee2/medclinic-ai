using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class RadiologyStudyConfiguration : IEntityTypeConfiguration<RadiologyStudy>
{
    public void Configure(EntityTypeBuilder<RadiologyStudy> builder)
    {
        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.StudyType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rs => rs.BodyPart)
            .HasMaxLength(100);

        builder.Property(rs => rs.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(rs => rs.Findings)
            .HasMaxLength(4000);

        builder.Property(rs => rs.Impression)
            .HasMaxLength(2000);

        builder.Property(rs => rs.AiSummary)
            .HasMaxLength(4000);

        builder.Property(rs => rs.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(rs => new { rs.ClinicId, rs.PatientId });

        builder.HasOne(rs => rs.Patient)
            .WithMany()
            .HasForeignKey(rs => rs.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rs => rs.Doctor)
            .WithMany()
            .HasForeignKey(rs => rs.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(rs => rs.Images)
            .WithOne(i => i.RadiologyStudy)
            .HasForeignKey(i => i.RadiologyStudyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
