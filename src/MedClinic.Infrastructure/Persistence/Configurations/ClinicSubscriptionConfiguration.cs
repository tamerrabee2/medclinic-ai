using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class ClinicSubscriptionConfiguration : IEntityTypeConfiguration<ClinicSubscription>
{
    public void Configure(EntityTypeBuilder<ClinicSubscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.PlanCodeSnapshot)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CurrencySnapshot)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(s => s.MonthlyPriceSnapshot)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.AnnualPriceSnapshot)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.CancellationReason)
            .HasMaxLength(500);

        builder.HasIndex(s => new { s.ClinicId, s.IsActive });
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.BillingStatus);

        builder.HasOne(s => s.Clinic)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SubscriptionPlan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
