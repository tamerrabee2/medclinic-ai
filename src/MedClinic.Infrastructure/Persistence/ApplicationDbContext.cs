using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options),
      IApplicationDbContext
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicMember> ClinicMembers => Set<ClinicMember>();
    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<LabResultItem> LabResultItems => Set<LabResultItem>();
    public DbSet<RadiologyStudy> RadiologyStudies => Set<RadiologyStudy>();
    public DbSet<MedicalImage> MedicalImages => Set<MedicalImage>();
    public DbSet<MedicalAnnotation> MedicalAnnotations => Set<MedicalAnnotation>();
    public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();
    public DbSet<AIConversation> AIConversations => Set<AIConversation>();
    public DbSet<AIConversationMessage> AIConversationMessages => Set<AIConversationMessage>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global soft-delete filter for TenantEntity types
        builder.Entity<Patient>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Doctor>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Visit>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Prescription>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<LabOrder>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<LabResult>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RadiologyStudy>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ClinicMember>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is MedClinic.Domain.Common.BaseEntity entity)
            {
                if (entry.State == EntityState.Modified)
                    entity.UpdatedAt = now;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
