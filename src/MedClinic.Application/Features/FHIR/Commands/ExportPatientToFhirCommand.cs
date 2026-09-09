using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;

namespace MedClinic.Application.Features.FHIR.Commands;

public record ExportPatientToFhirCommand(Guid PatientId) : IRequest<Result<FhirSyncDto>>;

public record FhirSyncDto(Guid Id, string ResourceType, string ResourceId, FhirSyncStatus Status, DateTime? SyncedAt, string PayloadJson);

public class ExportPatientToFhirCommandHandler : IRequestHandler<ExportPatientToFhirCommand, Result<FhirSyncDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFhirProvider _fhir;

    public ExportPatientToFhirCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IFhirProvider fhir)
    {
        _context = context;
        _currentUser = currentUser;
        _fhir = fhir;
    }

    public async Task<Result<FhirSyncDto>> Handle(ExportPatientToFhirCommand request, CancellationToken cancellationToken)
    {
        var result = await _fhir.ExportPatientAsync(request.PatientId, cancellationToken);

        var record = new FhirSyncRecord
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            ResourceType = result.ResourceType,
            ResourceId = result.ResourceId,
            ExternalSystem = "FHIR",
            PayloadJson = result.PayloadJson,
            Direction = FhirSyncDirection.Export,
            Status = result.Success ? FhirSyncStatus.Synced : FhirSyncStatus.Failed,
            ErrorMessage = result.ErrorMessage,
            SyncedAt = result.Success ? DateTime.UtcNow : null
        };

        _context.FhirSyncRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<FhirSyncDto>.Success(new FhirSyncDto(record.Id, record.ResourceType, record.ResourceId, record.Status, record.SyncedAt, record.PayloadJson));
    }
}
