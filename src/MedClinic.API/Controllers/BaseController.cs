using MedClinic.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Success<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.SuccessResult(data, message));

    protected IActionResult Created<T>(T data, string? message = null)
        => StatusCode(201, ApiResponse<T>.SuccessResult(data, message));

    protected new IActionResult NotFound(string message)
        => base.NotFound(ApiResponse<object>.ErrorResult(message));

    protected new IActionResult BadRequest(string message)
        => base.BadRequest(ApiResponse<object>.ErrorResult(message));

    protected new IActionResult Unauthorized(string message = "Unauthorized")
        => base.Unauthorized(ApiResponse<object>.ErrorResult(message));

    protected IActionResult Forbidden(string message = "Access denied.")
        => StatusCode(403, ApiResponse<object>.ErrorResult(message));

    protected Guid CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }
}
