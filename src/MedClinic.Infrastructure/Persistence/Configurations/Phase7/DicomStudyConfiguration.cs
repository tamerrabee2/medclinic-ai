using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations.Phase7;

public class DicomStudyConfiguration : IEntityTypeConfiguration<DicomStudy>
{
    public void Configure(EntityTypeBuilder<DicomStudy> builder)
    {
        builder.ToTable("DicomStudies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudyInstanceUid).IsRequired().HasMaxLength(128);
        builder.HasIndex(x => x.StudyInstanceUid).IsUnique();
        builder.Property(x => x.Modality).IsRequired().HasMaxLength(16);
        builder.Property(x => x.AccessionNumber).HasMaxLength(64);
        builder.Property(x => x.StudyDescription).HasMaxLength(256);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.Series)
            .WithOne(s => s.DicomStudy)
            .HasForeignKey(s => s.DicomStudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PatientId, x.StudyDate });
        builder.HasIndex(x => new { x.ClinicId, x.Modality });
    }
}

public class DicomSeriesConfiguration : IEntityTypeConfiguration<DicomSeries>
{
    public void Configure(EntityTypeBuilder<DicomSeries> builder)
    {
        builder.ToTable("DicomSeries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SeriesInstanceUid).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Modality).IsRequired().HasMaxLength(16);
        builder.Property(x => x.SeriesDescription).HasMaxLength(256);
        builder.Property(x => x.BodyPartExamined).HasMaxLength(64);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.Instances)
            .WithOne(i => i.DicomSeries)
            .HasForeignKey(i => i.DicomSeriesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DicomInstanceConfiguration : IEntityTypeConfiguration<DicomInstance>
{
    public void Configure(EntityTypeBuilder<DicomInstance> builder)
    {
        builder.ToTable("DicomInstances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SopInstanceUid).IsRequired().HasMaxLength(128);
        builder.Property(x => x.SopClassUid).IsRequired().HasMaxLength(128);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
