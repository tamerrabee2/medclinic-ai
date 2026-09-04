using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class BodyMapAnnotationConfiguration : IEntityTypeConfiguration<BodyMapAnnotation>
{
    public void Configure(EntityTypeBuilder<BodyMapAnnotation> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Region).IsRequired().HasMaxLength(50);
        b.Property(x => x.Side).HasMaxLength(20);
        b.Property(x => x.Symptom).HasMaxLength(500);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.Diagnosis).HasMaxLength(500);
        b.Property(x => x.MarkerColor).HasMaxLength(20).HasDefaultValue("#EF4444");
        b.Property(x => x.PositionX).HasColumnType("double precision");
        b.Property(x => x.PositionY).HasColumnType("double precision");

        b.HasIndex(x => x.VisitId);
        b.HasIndex(x => new { x.ClinicId, x.PatientId });

        b.HasOne(x => x.Visit)
            .WithMany()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
