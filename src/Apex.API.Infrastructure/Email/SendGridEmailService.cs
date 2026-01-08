using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Apex.API.Core.Interfaces;

namespace Apex.API.Infrastructure.Email;

/// <summary>
/// SendGrid email service implementation
/// </summary>
public class SendGridEmailService : IEmailService
{
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly EmailOptions _options;
    private readonly ISendGridClient _sendGridClient;

    public SendGridEmailService(
        ILogger<SendGridEmailService> logger,
        IOptions<EmailOptions> options,
        ISendGridClient sendGridClient)
    {
        _logger = logger;
        _options = options.Value;
        _sendGridClient = sendGridClient;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email disabled. Would send to {Email}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody ?? htmlBody, htmlBody);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}: {Subject}", toEmail, subject);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Body: {Body}",
                    toEmail, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}: {Subject}", toEmail, subject);
            throw;
        }
    }

    public async Task SendEmailAsync(
        IEnumerable<(string Email, string Name)> recipients,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var (email, name) in recipients)
        {
            await SendEmailAsync(email, name, subject, htmlBody, plainTextBody, cancellationToken);
        }
    }

    public Task SendTemplatedEmailAsync<TModel>(
        string toEmail,
        string toName,
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class
    {
        // Template rendering handled by separate service
        throw new NotImplementedException("Use IEmailTemplateService to render templates first");
    }

    public Task SendTemplatedEmailAsync<TModel>(
        IEnumerable<(string Email, string Name)> recipients,
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class
    {
        throw new NotImplementedException("Use IEmailTemplateService to render templates first");
    }
}
