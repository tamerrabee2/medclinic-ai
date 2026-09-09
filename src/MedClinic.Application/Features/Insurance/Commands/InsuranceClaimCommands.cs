using MediatR;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Application.Common.Models;
using MedClinic.Domain.Entities;

namespace MedClinic.Application.Features.Insurance.Commands;

public record SubmitInsuranceClaimCommand(Guid PatientId, Guid InvoiceId, Guid PolicyId, string PayloadJson) : IRequest<Result<InsuranceClaimDto>>;
public record CheckInsuranceClaimStatusCommand(Guid ClaimId) : IRequest<Result<InsuranceClaimDto>>;

public record InsuranceClaimDto(Guid Id, string ClaimNumber, string Status, decimal ClaimedAmount, decimal ApprovedAmount, DateTime? SubmittedAt, string PayloadJson, string? ResponseJson);

public class SubmitInsuranceClaimCommandHandler : IRequestHandler<SubmitInsuranceClaimCommand, Result<InsuranceClaimDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IInsuranceProviderGateway _gateway;

    public SubmitInsuranceClaimCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IInsuranceProviderGateway gateway)
    {
        _context = context;
        _currentUser = currentUser;
        _gateway = gateway;
    }

    public async Task<Result<InsuranceClaimDto>> Handle(SubmitInsuranceClaimCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FindAsync(new object?[] { request.InvoiceId }, cancellationToken);
        if (invoice is null) return Result<InsuranceClaimDto>.Failure("Invoice not found.");

        var result = await _gateway.SubmitClaimAsync(request.PatientId, request.InvoiceId, request.PayloadJson, cancellationToken);

        var claim = new InsuranceClaim
        {
            ClinicId = _currentUser.ClinicId!.Value,
            PatientId = request.PatientId,
            InvoiceId = request.InvoiceId,
            InsurancePolicyId = request.PolicyId,
            ClaimNumber = result.ClaimNumber,
            ClaimedAmount = invoice.TotalAmount,
            ApprovedAmount = 0,
            Status = result.Success ? ClaimStatus.Submitted : ClaimStatus.Rejected,
            PayloadJson = request.PayloadJson,
            SubmittedAt = result.Success ? DateTime.UtcNow : null
        };

        _context.InsuranceClaims.Add(claim);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<InsuranceClaimDto>.Success(new InsuranceClaimDto(claim.Id, claim.ClaimNumber, claim.Status.ToString(), claim.ClaimedAmount, claim.ApprovedAmount, claim.SubmittedAt, claim.PayloadJson, claim.ResponseJson));
    }
}

public class CheckInsuranceClaimStatusCommandHandler : IRequestHandler<CheckInsuranceClaimStatusCommand, Result<InsuranceClaimDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IInsuranceProviderGateway _gateway;

    public CheckInsuranceClaimStatusCommandHandler(IApplicationDbContext context, IInsuranceProviderGateway gateway)
    {
        _context = context;
        _gateway = gateway;
    }

    public async Task<Result<InsuranceClaimDto>> Handle(CheckInsuranceClaimStatusCommand request, CancellationToken cancellationToken)
    {
        var claim = await _context.InsuranceClaims.FindAsync(new object?[] { request.ClaimId }, cancellationToken);
        if (claim is null) return Result<InsuranceClaimDto>.Failure("Claim not found.");

        var result = await _gateway.CheckClaimStatusAsync(claim.ClaimNumber, cancellationToken);
        claim.Status = Enum.TryParse<ClaimStatus>(result.Status, out var parsed) ? parsed : ClaimStatus.Submitted;
        claim.ApprovedAmount = result.ApprovedAmount;
        claim.ResponseJson = result.ResponseJson;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<InsuranceClaimDto>.Success(new InsuranceClaimDto(claim.Id, claim.ClaimNumber, claim.Status.ToString(), claim.ClaimedAmount, claim.ApprovedAmount, claim.SubmittedAt, claim.PayloadJson, claim.ResponseJson));
    }
}
