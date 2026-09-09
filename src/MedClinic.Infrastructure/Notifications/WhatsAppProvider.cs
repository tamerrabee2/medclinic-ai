using System.Text;
using System.Text.Json;
using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Notifications;

/// <summary>
/// WhatsApp Business API provider (Meta Cloud API).
/// Configure WHATSAPP_PHONE_NUMBER_ID and WHATSAPP_ACCESS_TOKEN in environment.
/// </summary>
public class WhatsAppProvider : IWhatsAppProvider
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppProvider> _logger;

    public WhatsAppProvider(HttpClient http, IConfiguration config, ILogger<WhatsAppProvider> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string?> SendMessageAsync(string toPhone, string message, CancellationToken ct = default)
    {
        var phoneId = _config["WHATSAPP_PHONE_NUMBER_ID"];
        var token = _config["WHATSAPP_ACCESS_TOKEN"];

        if (string.IsNullOrEmpty(phoneId) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("WhatsApp not configured. Message to {Phone} skipped.", toPhone);
            return null;
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhone.TrimStart('+'),
            type = "text",
            text = new { body = message }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.facebook.com/v18.0/{phoneId}/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("WhatsApp send failed: {Error}", err);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString();
    }

    public async Task<string?> SendTemplateAsync(string toPhone, string templateName,
        Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        var phoneId = _config["WHATSAPP_PHONE_NUMBER_ID"];
        var token = _config["WHATSAPP_ACCESS_TOKEN"];

        if (string.IsNullOrEmpty(phoneId) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("WhatsApp not configured. Template {Template} to {Phone} skipped.", templateName, toPhone);
            return null;
        }

        var components = parameters.Count > 0
            ? new object[]
              {
                  new
                  {
                      type = "body",
                      parameters = parameters.Values
                          .Select(v => new { type = "text", text = v })
                          .ToArray()
                  }
              }
            : Array.Empty<object>();

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhone.TrimStart('+'),
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = "ar" },
                components
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.facebook.com/v18.0/{phoneId}/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString();
    }
}
