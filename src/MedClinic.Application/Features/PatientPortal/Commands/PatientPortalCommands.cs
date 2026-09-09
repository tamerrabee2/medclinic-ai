using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.PatientPortal.Commands;

public record ProvisionPatientPortalAccessCommand(Guid PatientId, string Email, string PasswordHash) : IRequest<Result<Guid>>;
public record SendPatientPortalMessageCommand(Guid PatientId, string SenderType, string Subject, string Body) : IRequest<Result<Guid>>;

public class ProvisionPatientPortalAccessCommandHandler : IRequestHandler<ProvisionPatientPortalAccessCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProvisionPatientPortalAccessCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(ProvisionPatientPortalAccessCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.PatientPortalAccesses
            .FirstOrDefaultAsync(x => x.PatientId == request.PatientId && x.ClinicId == _currentUser.ClinicId, cancellationToken);

        if (existing is not null)
        {
            existing.Email = request.Email;
            existing.PasswordHash = request.PasswordHash;
            existing.IsActive = true;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(existing.Id);
        }

        var access = new PatientPortalAccess
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
            IsActive = true
        };

        _context.PatientPortalAccesses.Add(access);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(access.Id);
    }
}

public class SendPatientPortalMessageCommandHandler : IRequestHandler<SendPatientPortalMessageCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SendPatientPortalMessageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(SendPatientPortalMessageCommand request, CancellationToken cancellationToken)
    {
        var msg = new PatientPortalMessage
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            UserId = _currentUser.UserId,
            SenderType = request.SenderType,
            Subject = request.Subject,
            Body = request.Body
        };

        _context.PatientPortalMessages.Add(msg);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(msg.Id);
    }
}
