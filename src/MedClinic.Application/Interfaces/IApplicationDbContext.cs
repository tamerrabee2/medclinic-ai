using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Clinic> Clinics { get; }
    DbSet<ClinicMember> ClinicMembers { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Visit> Visits { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<LabOrder> LabOrders { get; }
    DbSet<LabResult> LabResults { get; }
    DbSet<LabResultItem> LabResultItems { get; }
    DbSet<RadiologyStudy> RadiologyStudies { get; }
    DbSet<MedicalImage> MedicalImages { get; }
    DbSet<MedicalAnnotation> MedicalAnnotations { get; }
    DbSet<AIAnalysis> AIAnalyses { get; }
    DbSet<AIConversation> AIConversations { get; }
    DbSet<AIConversationMessage> AIConversationMessages { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
