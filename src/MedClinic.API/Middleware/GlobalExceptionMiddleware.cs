using MedClinic.Shared.Common;
using System.Text.Json;

namespace MedClinic.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        if (exception is MedClinic.Domain.Exceptions.ConsentRequiredException consentEx)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            var problem = new
            {
                type = "consent_required",
                title = $"{consentEx.ConsentType} consent is required",
                status = StatusCodes.Status403Forbidden,
                consentType = consentEx.ConsentType.ToString(),
                detail = consentEx.Message,
                traceId
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
            return;
        }

        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            TraceId = traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
