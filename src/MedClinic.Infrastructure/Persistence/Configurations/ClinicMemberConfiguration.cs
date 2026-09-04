using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class ClinicMemberConfiguration : IEntityTypeConfiguration<ClinicMember>
{
    public void Configure(EntityTypeBuilder<ClinicMember> builder)
    {
        builder.ToTable("ClinicMembers");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ClinicId, x.UserId }).IsUnique();

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.Clinic)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ClinicMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
