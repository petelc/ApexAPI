using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Apex.API.Core.Interfaces;

namespace Apex.API.Infrastructure.Email;

/// <summary>
/// Console email service for development/testing
/// Writes emails to console instead of sending
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;
    private readonly EmailOptions _options;

    public ConsoleEmailService(
        ILogger<ConsoleEmailService> logger,
        IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """
            ================== EMAIL (Console Mode) ==================
            From: {FromName} <{FromEmail}>
            To: {ToName} <{ToEmail}>
            Subject: {Subject}
            
            {Body}
            ==========================================================
            """,
            _options.FromName,
            _options.FromEmail,
            toName,
            toEmail,
            subject,
            plainTextBody ?? StripHtml(htmlBody));

        return Task.CompletedTask;
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
        _logger.LogInformation(
            """
            ================== TEMPLATED EMAIL (Console Mode) ==================
            From: {FromName} <{FromEmail}>
            To: {ToName} <{ToEmail}>
            Template: {Template}
            Model: {Model}
            =====================================================================
            """,
            _options.FromName,
            _options.FromEmail,
            toName,
            toEmail,
            templateName,
            System.Text.Json.JsonSerializer.Serialize(model, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            }));

        return Task.CompletedTask;
    }

    public async Task SendTemplatedEmailAsync<TModel>(
        IEnumerable<(string Email, string Name)> recipients,
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class
    {
        foreach (var (email, name) in recipients)
        {
            await SendTemplatedEmailAsync(email, name, templateName, model, cancellationToken);
        }
    }

    private static string StripHtml(string html)
    {
        // Simple HTML stripping for console display
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}
