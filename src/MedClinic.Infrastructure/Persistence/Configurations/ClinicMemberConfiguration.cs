using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class ClinicMemberConfiguration : IEntityTypeConfiguration<ClinicMember>
{
    public void Configure(EntityTypeBuilder<ClinicMember> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.HasIndex(cm => new { cm.ClinicId, cm.UserId }).IsUnique();

        builder.Property(cm => cm.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(cm => cm.Clinic)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany(u => u.ClinicMemberships)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
