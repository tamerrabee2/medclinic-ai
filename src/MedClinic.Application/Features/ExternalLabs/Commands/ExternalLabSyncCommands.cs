using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;

namespace MedClinic.Application.Features.ExternalLabs.Commands;

public record SubmitExternalLabOrderCommand(Guid PatientId, Guid LabOrderId, Guid ProviderId, string PayloadJson) : IRequest<Result<ExternalLabSyncDto>>;
public record FetchExternalLabResultCommand(Guid SyncId) : IRequest<Result<ExternalLabSyncDto>>;

public record ExternalLabSyncDto(Guid Id, string ExternalOrderId, string Status, DateTime? SyncedAt, string PayloadJson, string? ResultJson);

public class SubmitExternalLabOrderCommandHandler : IRequestHandler<SubmitExternalLabOrderCommand, Result<ExternalLabSyncDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IExternalLabProvider _provider;

    public SubmitExternalLabOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IExternalLabProvider provider)
    {
        _context = context;
        _currentUser = currentUser;
        _provider = provider;
    }

    public async Task<Result<ExternalLabSyncDto>> Handle(SubmitExternalLabOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await _provider.SubmitOrderAsync(request.PatientId, request.LabOrderId, request.PayloadJson, cancellationToken);

        var sync = new ExternalLabSync
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            LabOrderId = request.LabOrderId,
            ExternalLabProviderId = request.ProviderId,
            ExternalOrderId = result.ExternalOrderId,
            Status = result.Success ? "Submitted" : "Failed",
            PayloadJson = request.PayloadJson,
            SyncedAt = result.Success ? DateTime.UtcNow : null
        };

        _context.ExternalLabSyncs.Add(sync);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ExternalLabSyncDto>.Success(new ExternalLabSyncDto(sync.Id, sync.ExternalOrderId, sync.Status, sync.SyncedAt, sync.PayloadJson, sync.ResultJson));
    }
}

public class FetchExternalLabResultCommandHandler : IRequestHandler<FetchExternalLabResultCommand, Result<ExternalLabSyncDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IExternalLabProvider _provider;

    public FetchExternalLabResultCommandHandler(IApplicationDbContext context, IExternalLabProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<Result<ExternalLabSyncDto>> Handle(FetchExternalLabResultCommand request, CancellationToken cancellationToken)
    {
        var sync = await _context.ExternalLabSyncs.FindAsync(new object?[] { request.SyncId }, cancellationToken);
        if (sync is null) return Result<ExternalLabSyncDto>.Failure("External lab sync not found.");

        var result = await _provider.FetchResultAsync(sync.ExternalOrderId, cancellationToken);
        sync.Status = result.Status;
        sync.ResultJson = result.ResultJson;
        sync.SyncedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ExternalLabSyncDto>.Success(new ExternalLabSyncDto(sync.Id, sync.ExternalOrderId, sync.Status, sync.SyncedAt, sync.PayloadJson, sync.ResultJson));
    }
}
