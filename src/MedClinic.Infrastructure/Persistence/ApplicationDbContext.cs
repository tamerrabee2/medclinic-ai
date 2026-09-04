using MedClinic.Domain.Common;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace MedClinic.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ── DbSets ────────────────────────────────────────────────────────────────

    // Identity
    public DbSet<User>            Users           { get; set; }
    public DbSet<RefreshToken>    RefreshTokens   { get; set; }

    // Clinic & Staff
    public DbSet<Clinic>          Clinics         { get; set; }
    public DbSet<ClinicMember>    ClinicMembers   { get; set; }
    public DbSet<Doctor>          Doctors         { get; set; }

    // Patients
    public DbSet<Patient>         Patients        { get; set; }
    public DbSet<PatientAllergy>  PatientAllergies { get; set; }

    // Appointments
    public DbSet<Appointment>     Appointments    { get; set; }
    public DbSet<AppointmentSlot> AppointmentSlots { get; set; }

    // Visits / Medical Records
    public DbSet<Visit>           Visits          { get; set; }
    public DbSet<Prescription>    Prescriptions   { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<LabOrder>        LabOrders       { get; set; }
    public DbSet<LabOrderItem>    LabOrderItems   { get; set; }
    public DbSet<LabResult>       LabResults      { get; set; }
    public DbSet<RadiologyStudy>  RadiologyStudies { get; set; }
    public DbSet<RadiologyImage>  RadiologyImages  { get; set; }

    // Billing
    public DbSet<Invoice>         Invoices        { get; set; }
    public DbSet<InvoiceItem>     InvoiceItems    { get; set; }
    public DbSet<Payment>         Payments        { get; set; }

    // AI
    public DbSet<AIConversation>  AIConversations { get; set; }
    public DbSet<AIMessage>       AIMessages      { get; set; }

    // System
    public DbSet<Notification>    Notifications   { get; set; }
    public DbSet<AuditLog>        AuditLogs       { get; set; }

    // ── Model Configuration ───────────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ── Global Query Filters (Soft Delete) ──
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [modelBuilder]);
            }
        }
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder builder)
        where T : class, ISoftDelete
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    // ── Audit Interceptor (CreatedAt / UpdatedAt / CreatedBy) ─────────────────────────────

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var now    = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
                if (entry.Entity.CreatedBy == Guid.Empty)
                    entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
