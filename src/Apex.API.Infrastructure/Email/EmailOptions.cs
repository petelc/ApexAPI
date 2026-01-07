namespace Apex.API.Infrastructure.Email;

/// <summary>
/// Email service configuration options
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// Email provider: SendGrid, Smtp, or Console (for testing)
    /// </summary>
    public string Provider { get; set; } = "Console";

    /// <summary>
    /// From email address
    /// </summary>
    public string FromEmail { get; set; } = "noreply@apex.com";

    /// <summary>
    /// From display name
    /// </summary>
    public string FromName { get; set; } = "APEX Platform";

    /// <summary>
    /// SendGrid API key (if using SendGrid)
    /// </summary>
    public string? SendGridApiKey { get; set; }

    /// <summary>
    /// SMTP settings (if using SMTP)
    /// </summary>
    public SmtpSettings Smtp { get; set; } = new();

    /// <summary>
    /// Base URL for links in emails
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:5000";

    /// <summary>
    /// Enable email notifications globally
    /// </summary>
    public bool Enabled { get; set; } = true;
}

public class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
}
