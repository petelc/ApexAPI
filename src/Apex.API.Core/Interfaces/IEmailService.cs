namespace Apex.API.Core.Interfaces;

/// <summary>
/// Email service for sending notifications
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email to single recipient
    /// </summary>
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email to multiple recipients
    /// </summary>
    Task SendEmailAsync(
        IEnumerable<(string Email, string Name)> recipients,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email using template
    /// </summary>
    Task SendTemplatedEmailAsync<TModel>(
        string toEmail,
        string toName,
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class;

    /// <summary>
    /// Send email to multiple recipients using template
    /// </summary>
    Task SendTemplatedEmailAsync<TModel>(
        IEnumerable<(string Email, string Name)> recipients,
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class;
}
