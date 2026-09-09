using MedClinic.Application.Common.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MedClinic.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        if (exception is MedClinic.Domain.Exceptions.ConsentRequiredException consentEx)
        {
            var consentProblem = new ProblemDetails
            {
                Type = "consent_required",
                Status = StatusCodes.Status403Forbidden,
                Title = $"{consentEx.ConsentType} consent is required",
                Detail = consentEx.Message,
                Extensions = { ["consentType"] = consentEx.ConsentType.ToString() }
            };
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(consentProblem, cancellationToken);
            return true;
        }

        var (statusCode, title) = exception switch
        {
            ValidationException ve => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        var errors = exception is ValidationException validationEx
            ? validationEx.Errors
            : new[] { exception.Message };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Extensions = { ["errors"] = errors }
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
