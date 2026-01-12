namespace Apex.Domain.Services;

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send password reset email
    /// </summary>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetUrl,
        CancellationToken cancellationToken = default);

    // Add other email methods as needed
}
