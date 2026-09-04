using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.HasKey(i => i.Id);
        b.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        b.HasIndex(i => new { i.ClinicId, i.InvoiceNumber }).IsUnique();
        b.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        b.Property(i => i.PaidAmount).HasColumnType("decimal(18,2)");
        b.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
        b.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        b.Property(i => i.SubTotal).HasColumnType("decimal(18,2)");
        b.Property(i => i.Currency).HasMaxLength(10).HasDefaultValue("USD");
        b.Property(i => i.Status).HasConversion<string>();

        b.HasMany(i => i.Items)
            .WithOne(it => it.Invoice)
            .HasForeignKey(it => it.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
