using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class MedicalAnnotationConfiguration : IEntityTypeConfiguration<MedicalAnnotation>
{
    public void Configure(EntityTypeBuilder<MedicalAnnotation> builder)
    {
        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.AnnotationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ma => ma.Color)
            .HasMaxLength(20);

        builder.Property(ma => ma.Text)
            .HasMaxLength(1000);

        // Store coordinates as JSON
        builder.Property(ma => ma.CoordinatesJson)
            .HasColumnType("jsonb");

        builder.HasOne(ma => ma.MedicalImage)
            .WithMany(mi => mi.Annotations)
            .HasForeignKey(ma => ma.MedicalImageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
