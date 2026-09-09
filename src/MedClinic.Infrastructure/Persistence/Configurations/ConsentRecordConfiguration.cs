using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.RevocationReason)
            .HasMaxLength(500);

        builder.Property(c => c.IpAddress)
            .HasMaxLength(64);

        builder.HasIndex(c => new { c.ClinicId, c.PatientId, c.ConsentType });

        builder.HasOne(c => c.Patient)
            .WithMany()
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.WitnessUser)
            .WithMany()
            .HasForeignKey(c => c.WitnessUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.GrantedByUser)
            .WithMany()
            .HasForeignKey(c => c.GrantedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.RevokedByUser)
            .WithMany()
            .HasForeignKey(c => c.RevokedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.AuditEvents)
            .WithOne(a => a.ConsentRecord)
            .HasForeignKey(a => a.ConsentRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
