using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.Application.Features.FHIR.Queries;

public record GetPatientFhirHistoryQuery(Guid PatientId) : IRequest<Result<List<FhirHistoryDto>>>;

public record FhirHistoryDto(Guid Id, string ResourceType, string ResourceId, string Direction, string Status, DateTime? SyncedAt);

public class GetPatientFhirHistoryQueryHandler : IRequestHandler<GetPatientFhirHistoryQuery, Result<List<FhirHistoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPatientFhirHistoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FhirHistoryDto>>> Handle(GetPatientFhirHistoryQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.FhirSyncRecords
            .Where(r => r.PatientId == request.PatientId && r.ClinicId == _currentUser.ClinicId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new FhirHistoryDto(r.Id, r.ResourceType, r.ResourceId, r.Direction.ToString(), r.Status.ToString(), r.SyncedAt))
            .ToListAsync(cancellationToken);

        return Result<List<FhirHistoryDto>>.Success(items);
    }
}
