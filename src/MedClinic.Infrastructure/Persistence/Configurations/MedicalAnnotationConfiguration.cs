using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class MedicalAnnotationConfiguration : IEntityTypeConfiguration<MedicalAnnotation>
{
    public void Configure(EntityTypeBuilder<MedicalAnnotation> b)
    {
        b.HasKey(a => a.Id);
        b.Property(a => a.Type).IsRequired().HasMaxLength(50);
        b.Property(a => a.Color).HasMaxLength(20).HasDefaultValue("#FF0000");
        b.Property(a => a.CoordinatesJson).IsRequired().HasColumnType("text");
        b.Property(a => a.Text).HasMaxLength(500);
        b.Property(a => a.MeasurementValue).HasColumnType("double precision");
        b.Property(a => a.AIConfidence).HasColumnType("double precision");

        b.HasIndex(a => a.MedicalImageId);
        b.HasIndex(a => new { a.MedicalImageId, a.DoctorId });

        b.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
