using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class ConsentAuditEventConfiguration : IEntityTypeConfiguration<ConsentAuditEvent>
{
    public void Configure(EntityTypeBuilder<ConsentAuditEvent> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.Details)
            .HasMaxLength(1000);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(64);

        builder.HasIndex(a => new { a.ClinicId, a.PatientId, a.Timestamp });
        builder.HasIndex(a => new { a.ClinicId, a.ConsentRecordId });

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.PerformedByUser)
            .WithMany()
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
