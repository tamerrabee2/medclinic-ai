using MedClinic.Domain.Common;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Core ──────────────────────────────────────────────────────────────
    public DbSet<Clinic>       Clinics       { get; set; }
    public DbSet<ClinicMember> ClinicMembers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog>     AuditLogs     { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // ── Clinical ──────────────────────────────────────────────────────────
    public DbSet<Patient>      Patients      { get; set; }
    public DbSet<Doctor>       Doctors       { get; set; }
    public DbSet<Appointment>  Appointments  { get; set; }
    public DbSet<Visit>        Visits        { get; set; }

    // ── Medical Records ─────────────────────────────────────────────────
    public DbSet<Prescription>     Prescriptions     { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<LabOrder>         LabOrders         { get; set; }
    public DbSet<LabResult>        LabResults        { get; set; }
    public DbSet<LabResultItem>    LabResultItems    { get; set; }

    // ── Radiology ──────────────────────────────────────────────────────────
    public DbSet<RadiologyStudy>   RadiologyStudies  { get; set; }
    public DbSet<MedicalImage>     MedicalImages     { get; set; }
    public DbSet<AIAnalysis>       AIAnalyses        { get; set; }
    public DbSet<MedicalAnnotation> MedicalAnnotations { get; set; }

    // ── Billing ────────────────────────────────────────────────────────────
    public DbSet<Invoice>     Invoices     { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Payment>     Payments     { get; set; }

    // ── AI / Conversations ───────────────────────────────────────────────
    public DbSet<AIConversation>        AIConversations        { get; set; }
    public DbSet<AIConversationMessage> AIConversationMessages { get; set; }

    // ──────────────────────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Identity table renames ──
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // ── Global Soft Delete Filters ──
        builder.Entity<Clinic>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ClinicMember>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Patient>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Doctor>().HasQueryFilter(e => !e.IsDeleted);

        // ── Clinic ──
        builder.Entity<Clinic>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Slug).HasMaxLength(100).IsRequired();
        });

        // ── ClinicMember ──
        builder.Entity<ClinicMember>(e =>
        {
            e.HasIndex(cm => new { cm.ClinicId, cm.UserId });
            e.HasOne(cm => cm.Clinic).WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ClinicId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cm => cm.User).WithMany()
                .HasForeignKey(cm => cm.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── RefreshToken ──
        builder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(rt => rt.Token).IsUnique();
            e.HasOne(rt => rt.User).WithMany()
                .HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Patient ──
        builder.Entity<Patient>(e =>
        {
            e.HasIndex(p => new { p.ClinicId, p.NationalId });
            e.HasIndex(p => new { p.ClinicId, p.Phone });
        });

        // ── Doctor ──
        builder.Entity<Doctor>(e =>
        {
            e.HasIndex(d => new { d.ClinicId, d.UserId }).IsUnique();
            e.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Clinic).WithMany(c => c.Doctors)
                .HasForeignKey(d => d.ClinicId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Appointment ──
        builder.Entity<Appointment>(e =>
        {
            e.HasIndex(a => new { a.ClinicId, a.DoctorId, a.ScheduledAt });
            e.HasOne(a => a.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Doctor).WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Visit ──
        builder.Entity<Visit>(e =>
        {
            e.HasIndex(v => new { v.ClinicId, v.PatientId, v.VisitDate });
            e.OwnsOne(v => v.Vitals, vitals =>
            {
                vitals.Property(vt => vt.Temperature).HasColumnType("decimal(5,2)");
                vitals.Property(vt => vt.Weight).HasColumnType("decimal(6,2)");
                vitals.Property(vt => vt.Height).HasColumnType("decimal(5,2)");
                vitals.Property(vt => vt.BMI).HasColumnType("decimal(5,2)");
                vitals.Property(vt => vt.OxygenSaturation).HasColumnType("decimal(5,2)");
            });
            e.HasOne(v => v.Patient).WithMany(p => p.Visits)
                .HasForeignKey(v => v.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Doctor).WithMany(d => d.Visits)
                .HasForeignKey(v => v.DoctorId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Prescription ──
        builder.Entity<Prescription>(e =>
        {
            e.HasIndex(p => new { p.ClinicId, p.PatientId });
            e.HasOne(p => p.Patient).WithMany(pt => pt.Prescriptions)
                .HasForeignKey(p => p.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Doctor).WithMany()
                .HasForeignKey(p => p.DoctorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PrescriptionItem>(e =>
        {
            e.HasOne(i => i.Prescription).WithMany(p => p.Items)
                .HasForeignKey(i => i.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── LabOrder ──
        builder.Entity<LabOrder>(e =>
        {
            e.HasIndex(o => new { o.ClinicId, o.PatientId });
            e.HasIndex(o => new { o.ClinicId, o.Status });
            e.HasOne(o => o.Patient).WithMany(p => p.LabOrders)
                .HasForeignKey(o => o.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Doctor).WithMany()
                .HasForeignKey(o => o.DoctorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<LabResult>(e =>
        {
            e.HasOne(r => r.LabOrder).WithMany(o => o.Results)
                .HasForeignKey(r => r.LabOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LabResultItem>(e =>
        {
            e.HasOne(i => i.LabResult).WithMany(r => r.Items)
                .HasForeignKey(i => i.LabResultId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── RadiologyStudy ──
        builder.Entity<RadiologyStudy>(e =>
        {
            e.HasIndex(s => new { s.ClinicId, s.PatientId });
            e.HasIndex(s => s.AccessionNumber).IsUnique();
            e.HasOne(s => s.Patient).WithMany(p => p.RadiologyStudies)
                .HasForeignKey(s => s.PatientId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MedicalImage>(e =>
        {
            e.HasOne(i => i.RadiologyStudy).WithMany(s => s.Images)
                .HasForeignKey(i => i.RadiologyStudyId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AIAnalysis>(e =>
        {
            e.HasOne(a => a.RadiologyStudy).WithMany(s => s.AIAnalyses)
                .HasForeignKey(a => a.RadiologyStudyId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Invoice / Billing ──
        builder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => new { i.ClinicId, i.PatientId });
            e.Property(i => i.TotalAmount).HasColumnType("decimal(12,2)");
            e.Property(i => i.PaidAmount).HasColumnType("decimal(12,2)");
            e.HasOne(i => i.Patient).WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PatientId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InvoiceItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
            e.Property(i => i.TotalPrice).HasColumnType("decimal(10,2)");
            e.HasOne(i => i.Invoice).WithMany(inv => inv.Items)
                .HasForeignKey(i => i.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasColumnType("decimal(12,2)");
            e.HasOne(p => p.Invoice).WithMany(inv => inv.Payments)
                .HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── AI Conversations ──
        builder.Entity<AIConversation>(e =>
        {
            e.HasIndex(c => new { c.ClinicId, c.UserId });
            e.HasMany(c => c.Messages).WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ──
        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => new { a.ClinicId, a.EntityName, a.EntityId });
            e.HasIndex(a => a.UserId);
        });
    }

    // ──────────────────────────────────────────────────────────────────────
    // Auto-populate audit fields on SaveChanges
    // ──────────────────────────────────────────────────────────────────────
    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
