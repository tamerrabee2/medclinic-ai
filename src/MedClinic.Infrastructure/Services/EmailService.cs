using System.Net;
using System.Net.Mail;
using MedClinic.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedClinic.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly string? _smtpHost;
    private readonly int _smtpPort;
    private readonly string? _smtpUser;
    private readonly string? _smtpPass;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
        _smtpHost = config["Email:SmtpHost"];
        _smtpPort = int.TryParse(config["Email:SmtpPort"], out var port) ? port : 587;
        _smtpUser = config["Email:SmtpUser"];
        _smtpPass = config["Email:SmtpPass"];
        _fromEmail = config["Email:FromEmail"] ?? "no-reply@medclinic.ai";
        _fromName = config["Email:FromName"] ?? "MedClinic AI";
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));
        }

        if (!string.IsNullOrWhiteSpace(_smtpHost))
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = true
                };

                if (!string.IsNullOrWhiteSpace(_smtpUser) && !string.IsNullOrWhiteSpace(_smtpPass))
                {
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                }

                await client.SendMailAsync(message, ct);
                _logger.LogInformation("Dispatched email '{Subject}' to {Recipient}", subject, toEmail);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP to {Recipient}", toEmail);
                throw;
            }
        }

        // Development / Fallback logger (tokens are omitted from logs for security)
        _logger.LogInformation("[DEV EMAIL] Dispatched email '{Subject}' to {Recipient} via local simulation", subject, toEmail);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken ct = default)
    {
        const string subject = "MedClinic AI — Password Reset Request";
        var body = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;">
                    <h2 style="color: #0284c7;">Password Reset Request</h2>
                    <p>Hello,</p>
                    <p>We received a request to reset your password for your MedClinic AI account. Click the button below to establish a new password:</p>
                    <p style="margin: 30px 0;">
                        <a href="{resetLink}" style="background-color: #0284c7; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;">Reset Password</a>
                    </p>
                    <p style="color: #64748b; font-size: 0.9em;">This link will expire in 15 minutes. If you did not initiate this request, please disregard this email or contact your clinic administrator.</p>
                </div>
            </body>
            </html>
            """;

        await SendEmailAsync(toEmail, subject, body, ct);
    }
}
