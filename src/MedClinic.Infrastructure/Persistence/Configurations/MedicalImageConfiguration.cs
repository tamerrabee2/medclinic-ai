using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class MedicalImageConfiguration : IEntityTypeConfiguration<MedicalImage>
{
    public void Configure(EntityTypeBuilder<MedicalImage> builder)
    {
        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.FileName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(mi => mi.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(mi => mi.ContentType)
            .HasMaxLength(100);

        builder.Property(mi => mi.ImageType)
            .HasMaxLength(50);

        builder.Property(mi => mi.Description)
            .HasMaxLength(1000);

        builder.HasOne(mi => mi.RadiologyStudy)
            .WithMany(rs => rs.Images)
            .HasForeignKey(mi => mi.RadiologyStudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mi => mi.Annotations)
            .WithOne(a => a.MedicalImage)
            .HasForeignKey(a => a.MedicalImageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
