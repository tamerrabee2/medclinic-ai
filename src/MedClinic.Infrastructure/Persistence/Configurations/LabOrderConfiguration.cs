using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.ToTable("LabOrders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TestName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ClinicalInfo).HasMaxLength(2000);

        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OrderedAt);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.LabOrders)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Visit)
            .WithMany()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Results)
            .WithOne(x => x.LabOrder)
            .HasForeignKey(x => x.LabOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
