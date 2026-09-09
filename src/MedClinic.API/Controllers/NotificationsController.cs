using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MedClinic.Application.Features.Notifications.Commands;
using MedClinic.Domain.Entities;

namespace MedClinic.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Send a custom notification via chosen channel.</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendNotificationCommand(
            req.PatientId,
            req.Type,
            req.Channel,
            req.Recipient,
            req.Subject,
            req.Body), ct);
        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }
}

public record SendNotificationRequest(
    Guid? PatientId,
    NotificationType Type,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body
);
