using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Notifications;

/// <summary>Mock WhatsApp provider for development / testing.</summary>
public class MockWhatsAppProvider : IWhatsAppProvider
{
    private readonly ILogger<MockWhatsAppProvider> _logger;

    public MockWhatsAppProvider(ILogger<MockWhatsAppProvider> logger) => _logger = logger;

    public Task<string?> SendMessageAsync(string toPhone, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockWhatsApp] Message to {Phone}: {Message}", toPhone, message);
        return Task.FromResult<string?>($"mock-msg-{Guid.NewGuid():N}");
    }

    public Task<string?> SendTemplateAsync(string toPhone, string templateName,
        Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        _logger.LogInformation("[MockWhatsApp] Template '{Template}' to {Phone}", templateName, toPhone);
        return Task.FromResult<string?>($"mock-tpl-{Guid.NewGuid():N}");
    }
}
