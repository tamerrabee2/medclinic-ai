using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedClinic.Infrastructure.Persistence.Configurations.Phase7;

public class InsuranceProviderConfiguration : IEntityTypeConfiguration<InsuranceProvider>
{
    public void Configure(EntityTypeBuilder<InsuranceProvider> builder)
    {
        builder.ToTable("InsuranceProviders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.PayerCode).IsRequired().HasMaxLength(64);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
    {
        builder.ToTable("InsurancePolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PolicyNumber).IsRequired().HasMaxLength(128);
        builder.Property(x => x.MemberId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.CoveragePercentage).HasColumnType("numeric(5,2)");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
    }
}

public class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
{
    public void Configure(EntityTypeBuilder<InsuranceClaim> builder)
    {
        builder.ToTable("InsuranceClaims");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClaimNumber).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ClaimedAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ApprovedAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.PayloadJson).HasDefaultValue("{}");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.ClinicId, x.PatientId });
    }
}
