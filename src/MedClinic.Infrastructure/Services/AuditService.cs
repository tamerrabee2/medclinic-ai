using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        Guid? userId = null,
        Guid? clinicId = null,
        string? entityName = null,
        Guid? entityId = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request?.Headers.UserAgent.ToString();

            var log = new AuditLog
            {
                Action = action,
                UserId = userId,
                ClinicId = clinicId ?? Guid.Empty,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit logging should never crash the application
            _logger.LogError(ex, "Failed to write audit log for action: {Action}", action);
        }
    }
}
