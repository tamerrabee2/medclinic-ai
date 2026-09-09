using MedClinic.Application.Interfaces;
using MedClinic.Domain.Common;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace MedClinic.Infrastructure.Persistence;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
    MedClinic.Application.Interfaces.IApplicationDbContext,
    MedClinic.Application.Common.Interfaces.IApplicationDbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ITenantContext? _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null,
        ITenantContext? tenantContext = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantContext = tenantContext;
    }

    public Guid? CurrentClinicId => _tenantContext?.ClinicId;
    public bool IsSuperAdmin => _tenantContext?.IsSuperAdmin ?? false;

    // ── DbSets ────────────────────────────────────────────────────────────────

    // Identity
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Clinic & Staff
    public DbSet<Clinic> Clinics { get; set; } = null!;
    public DbSet<ClinicMember> ClinicMembers { get; set; } = null!;
    public DbSet<Doctor> Doctors { get; set; } = null!;

    // Patients
    public DbSet<Patient> Patients { get; set; } = null!;

    // Appointments
    public DbSet<Appointment> Appointments { get; set; } = null!;

    // Visits / Medical Records
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<Prescription> Prescriptions { get; set; } = null!;
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; } = null!;
    public DbSet<LabOrder> LabOrders { get; set; } = null!;
    public DbSet<LabResult> LabResults { get; set; } = null!;
    public DbSet<LabResultItem> LabResultItems { get; set; } = null!;
    public DbSet<RadiologyStudy> RadiologyStudies { get; set; } = null!;
    public DbSet<MedicalImage> MedicalImages { get; set; } = null!;
    public DbSet<MedicalAnnotation> MedicalAnnotations { get; set; } = null!;
    public DbSet<BodyMapAnnotation> BodyMapAnnotations { get; set; } = null!;
    public DbSet<DentalRecord> DentalRecords { get; set; } = null!;

    // AI
    public DbSet<AIAnalysis> AIAnalyses { get; set; } = null!;
    public DbSet<AIConversation> AIConversations { get; set; } = null!;
    public DbSet<AIConversationMessage> AIConversationMessages { get; set; } = null!;
    public DbSet<AiDecisionAudit> AiDecisionAudits { get; set; } = null!;

    // Billing
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    // System
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<ConsentRecord> ConsentRecords { get; set; } = null!;
    public DbSet<ConsentAuditEvent> ConsentAuditEvents { get; set; } = null!;

    // ── Model Configuration ───────────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ── Global Query Filters (Tenant Isolation & Soft Delete) ──
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(TenantEntity).IsAssignableFrom(clrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(clrType);
                method.Invoke(this, [modelBuilder]);
            }
            else if (typeof(BaseEntity).IsAssignableFrom(clrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(clrType);
                method.Invoke(null, [modelBuilder]);
            }
        }
    }

    private void SetTenantFilter<T>(ModelBuilder builder) where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && (IsSuperAdmin || _tenantContext == null || e.ClinicId == CurrentClinicId));
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : BaseEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    // ── Audit Interceptor (CreatedAt / UpdatedAt / CreatedBy) ─────────────────────────────

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
                if (!entry.Entity.CreatedBy.HasValue || entry.Entity.CreatedBy == Guid.Empty)
                    entry.Entity.CreatedBy = userId == Guid.Empty ? null : userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId == Guid.Empty ? null : userId;
            }
            else if (entry.State == EntityState.Deleted && (entry.Entity is ConsentRecord || entry.Entity is ConsentAuditEvent))
            {
                throw new InvalidOperationException("Hard deletion of consent compliance records is prohibited. Use revocation or soft delete.");
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor?.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
