using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class MedicalAnnotationConfiguration : IEntityTypeConfiguration<MedicalAnnotation>
{
    public void Configure(EntityTypeBuilder<MedicalAnnotation> builder)
    {
        builder.ToTable("MedicalAnnotations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.Property(x => x.Text).HasMaxLength(1000);
        // Coordinates stored as JSON
        builder.Property(x => x.CoordinatesJson)
            .HasColumnName("Coordinates")
            .HasColumnType("jsonb");

        builder.HasIndex(x => x.MedicalImageId);
        builder.HasIndex(x => x.DoctorId);

        builder.HasOne(x => x.MedicalImage)
            .WithMany(x => x.Annotations)
            .HasForeignKey(x => x.MedicalImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
