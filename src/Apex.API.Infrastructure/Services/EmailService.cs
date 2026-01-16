using Apex.API.Core.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Apex.Infrastructure.Services;

/// <summary>
/// Email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Reset Your Password - APEX";
            var body = GetPasswordResetEmailBody(userName, resetUrl);

            await SendEmailAsync(toEmail, subject, body, cancellationToken);

            _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        using var smtpClient = new System.Net.Mail.SmtpClient(_settings.SmtpServer, _settings.SmtpPort);
        smtpClient.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
        smtpClient.EnableSsl = _settings.EnableSsl;

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private string GetPasswordResetEmailBody(string userName, string resetUrl)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    color: #333;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    padding: 20px;
                }}
                .header {{
                    background-color: #1976d2;
                    color: white;
                    padding: 20px;
                    text-align: center;
                    border-radius: 5px 5px 0 0;
                }}
                .content {{
                    background-color: #f9f9f9;
                    padding: 30px;
                    border: 1px solid #ddd;
                    border-radius: 0 0 5px 5px;
                }}
                .button {{
                    display: inline-block;
                    padding: 12px 30px;
                    background-color: #1976d2;
                    color: white;
                    text-decoration: none;
                    border-radius: 5px;
                    margin: 20px 0;
                }}
                .footer {{
                    text-align: center;
                    margin-top: 20px;
                    color: #666;
                    font-size: 12px;
                }}
                .warning {{
                    background-color: #fff3cd;
                    border: 1px solid #ffc107;
                    padding: 15px;
                    border-radius: 5px;
                    margin: 20px 0;
                }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <div class=""header"">
                    <h1>Password Reset Request</h1>
                </div>
                <div class=""content"">
                    <p>Hello {userName},</p>
                    
                    <p>We received a request to reset your password for your APEX account.</p>
                    
                    <p>Click the button below to reset your password:</p>
                    
                    <div style=""text-align: center;"">
                        <a href=""{resetUrl}"" class=""button"">Reset Password</a>
                    </div>
                    
                    <div class=""warning"">
                        <strong>⚠️ Important Security Information:</strong>
                        <ul>
                            <li>This link will expire in 1 hour for security reasons</li>
                            <li>If you didn't request this reset, please ignore this email</li>
                            <li>Never share this link with anyone</li>
                        </ul>
                    </div>
                    
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style=""word-break: break-all; color: #1976d2;"">{resetUrl}</p>
                    
                    <p>If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.</p>
                    
                    <p>Best regards,<br>The APEX Team</p>
                </div>
                <div class=""footer"">
                    <p>This is an automated email. Please do not reply.</p>
                    <p>&copy; 2026 APEX. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, string? plainTextBody = null, CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var smtpClient = new System.Net.Mail.SmtpClient(_settings.SmtpServer, _settings.SmtpPort);
        smtpClient.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
        smtpClient.EnableSsl = _settings.EnableSsl;

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    public Task SendEmailAsync(IEnumerable<(string Email, string Name)> recipients, string subject, string htmlBody, string? plainTextBody = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SendTemplatedEmailAsync<TModel>(string toEmail, string toName, string templateName, TModel model, CancellationToken cancellationToken = default) where TModel : class
    {
        throw new NotImplementedException();
    }

    public Task SendTemplatedEmailAsync<TModel>(IEnumerable<(string Email, string Name)> recipients, string templateName, TModel model, CancellationToken cancellationToken = default) where TModel : class
    {
        throw new NotImplementedException();
    }

}

/// <summary>
/// Email settings configuration
/// </summary>
public class EmailSettings
{
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
    public required string SmtpServer { get; init; }
    public required int SmtpPort { get; init; }
    public required string SmtpUsername { get; init; }
    public required string SmtpPassword { get; init; }
    public bool EnableSsl { get; init; } = true;
}
