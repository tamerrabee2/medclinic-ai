using System.Text.Json;
using MedClinic.Domain.Entities;
using MedClinic.Domain.Enums;
using MedClinic.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public static readonly Guid BasicPlanId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ProPlanId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid EnterprisePlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(p => p.MonthlyPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.AnnualPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.FeaturesJson)
            .IsRequired()
            .HasDefaultValue("[]");

        builder.HasMany(p => p.Subscriptions)
            .WithOne(s => s.SubscriptionPlan)
            .HasForeignKey(s => s.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Default System Plans
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new SubscriptionPlan
            {
                Id = BasicPlanId,
                Code = "basic",
                Name = "Basic Clinic",
                Description = "Essential clinical features for solo practitioners and small clinics.",
                Tier = SubscriptionTier.Basic,
                MonthlyPrice = 49.00m,
                AnnualPrice = 490.00m,
                Currency = "USD",
                MaxDoctors = 3,
                MaxUsers = 5,
                MaxPatients = 500,
                MaxStorageBytes = 5L * 1024 * 1024 * 1024, // 5 GB
                MonthlyAiRequestsLimit = 100,
                MaxDicomStudiesMonthly = 0,
                FeaturesJson = JsonSerializer.Serialize(new[] { FeatureKey.PatientPortal }),
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new SubscriptionPlan
            {
                Id = ProPlanId,
                Code = "pro",
                Name = "Professional Clinic",
                Description = "Advanced features including AI Copilot, DICOM PACS, and lab integrations.",
                Tier = SubscriptionTier.Pro,
                MonthlyPrice = 149.00m,
                AnnualPrice = 1490.00m,
                Currency = "USD",
                MaxDoctors = 15,
                MaxUsers = 25,
                MaxPatients = 5000,
                MaxStorageBytes = 50L * 1024 * 1024 * 1024, // 50 GB
                MonthlyAiRequestsLimit = 1500,
                MaxDicomStudiesMonthly = 100,
                FeaturesJson = JsonSerializer.Serialize(new[]
                {
                    FeatureKey.AiCopilot,
                    FeatureKey.DicomPacs,
                    FeatureKey.ExternalLabs,
                    FeatureKey.PatientPortal,
                    FeatureKey.AdvancedReports
                }),
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = now,
                UpdatedAt = now
            },
            new SubscriptionPlan
            {
                Id = EnterprisePlanId,
                Code = "enterprise",
                Name = "Enterprise Hospital",
                Description = "Unlimited multi-specialty clinical operations with full API access and custom branding.",
                Tier = SubscriptionTier.Enterprise,
                MonthlyPrice = 499.00m,
                AnnualPrice = 4990.00m,
                Currency = "USD",
                MaxDoctors = 0, // 0 = unlimited
                MaxUsers = 0,
                MaxPatients = 0,
                MaxStorageBytes = 0,
                MonthlyAiRequestsLimit = 0,
                MaxDicomStudiesMonthly = 0,
                FeaturesJson = JsonSerializer.Serialize(FeatureKey.All),
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
