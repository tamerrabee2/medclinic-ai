using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class MedicalImageConfiguration : IEntityTypeConfiguration<MedicalImage>
{
    public void Configure(EntityTypeBuilder<MedicalImage> builder)
    {
        builder.ToTable("MedicalImages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.OriginalUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.AnnotatedUrl).HasMaxLength(500);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.Modality).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasIndex(x => x.RadiologyStudyId);

        builder.HasMany(x => x.Annotations)
            .WithOne(x => x.MedicalImage)
            .HasForeignKey(x => x.MedicalImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AIAnalyses)
            .WithOne(x => x.MedicalImage)
            .HasForeignKey(x => x.MedicalImageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
