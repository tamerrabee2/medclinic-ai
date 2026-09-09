using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.PatientPortal.Queries;

public record GetPatientPortalMessagesQuery(Guid PatientId) : IRequest<Result<List<PatientPortalMessageDto>>>;
public record PatientPortalMessageDto(Guid Id, string SenderType, string Subject, string Body, bool IsRead, DateTime CreatedAt);

public class GetPatientPortalMessagesQueryHandler : IRequestHandler<GetPatientPortalMessagesQuery, Result<List<PatientPortalMessageDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPatientPortalMessagesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<PatientPortalMessageDto>>> Handle(GetPatientPortalMessagesQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.PatientPortalMessages
            .Where(m => m.PatientId == request.PatientId && m.ClinicId == _currentUser.ClinicId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new PatientPortalMessageDto(m.Id, m.SenderType, m.Subject, m.Body, m.IsRead, m.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<PatientPortalMessageDto>>.Success(items);
    }
}
