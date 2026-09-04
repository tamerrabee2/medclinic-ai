using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> b)
    {
        b.HasKey(c => c.Id);
        b.Property(c => c.Name).IsRequired().HasMaxLength(200);
        b.Property(c => c.Slug).IsRequired().HasMaxLength(100);
        b.HasIndex(c => c.Slug).IsUnique();
        b.Property(c => c.Currency).HasMaxLength(10).HasDefaultValue("USD");
        b.Property(c => c.Timezone).HasMaxLength(100).HasDefaultValue("UTC");
        b.Property(c => c.InvoicePrefix).HasMaxLength(20).HasDefaultValue("INV");
        b.Property(c => c.TaxRate).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
        b.Property(c => c.DefaultAppointmentDuration).HasDefaultValue(30);
        b.Property(c => c.AllowOnlineBooking).HasDefaultValue(true);
    }
}
