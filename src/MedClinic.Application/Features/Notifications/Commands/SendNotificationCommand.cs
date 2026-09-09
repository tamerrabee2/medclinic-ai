using MediatR;
using MedClinic.Application.Common.Models;
using MedClinic.Application.Common.Interfaces;
using MedClinic.Domain.Entities;

namespace MedClinic.Application.Features.Notifications.Commands;

public record SendNotificationCommand(
    Guid? PatientId,
    NotificationType Type,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body
) : IRequest<Result<bool>>;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public SendNotificationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        await _notificationService.SendCustomAsync(
            _currentUser.ClinicId!.Value,
            request.PatientId,
            request.Type,
            request.Channel,
            request.Recipient,
            request.Subject,
            request.Body,
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
