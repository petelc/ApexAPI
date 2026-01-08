using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.Infrastructure.Email;
using Apex.API.Infrastructure.Email.Templates;

namespace Apex.API.Infrastructure.Jobs;

/// <summary>
/// Background job to send reminder emails before scheduled changes
/// </summary>
public class ChangeReminderJob
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ChangeReminderJob> _logger;
    private readonly EmailOptions _emailOptions;

    public ChangeReminderJob(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<ChangeReminderJob> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    /// <summary>
    /// Send reminder emails 24 hours before scheduled changes
    /// Runs every 15 minutes
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task SendReminders24HoursBefore()
    {
        _logger.LogInformation("📧 Checking for 24-hour change reminders...");

        try
        {
            var now = DateTime.UtcNow;
            var targetStart = now.AddHours(24);
            var targetEnd = targetStart.AddMinutes(15); // 15-minute window

            var allChanges = await _changeRequestRepository.ListAsync();

            // Find changes scheduled to start in ~24 hours
            var upcomingChanges = allChanges
                .Where(c => c.Status == ChangeRequestStatus.Scheduled &&
                           c.ScheduledStartDate.HasValue &&
                           c.ScheduledStartDate.Value >= targetStart &&
                           c.ScheduledStartDate.Value <= targetEnd)
                .ToList();

            if (!upcomingChanges.Any())
            {
                _logger.LogInformation("✅ No changes need 24-hour reminders");
                return;
            }

            _logger.LogInformation("📬 Sending 24-hour reminders for {Count} change(s)", upcomingChanges.Count);

            foreach (var change in upcomingChanges)
            {
                await SendReminderEmail(change, 24);
            }

            _logger.LogInformation("✅ Completed 24-hour reminders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SendReminders24HoursBefore job");
            throw;
        }
    }

    /// <summary>
    /// Send reminder emails 1 hour before scheduled changes
    /// Runs every 15 minutes
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task SendReminders1HourBefore()
    {
        _logger.LogInformation("📧 Checking for 1-hour change reminders...");

        try
        {
            var now = DateTime.UtcNow;
            var targetStart = now.AddHours(1);
            var targetEnd = targetStart.AddMinutes(15); // 15-minute window

            var allChanges = await _changeRequestRepository.ListAsync();

            // Find changes scheduled to start in ~1 hour
            var upcomingChanges = allChanges
                .Where(c => c.Status == ChangeRequestStatus.Scheduled &&
                           c.ScheduledStartDate.HasValue &&
                           c.ScheduledStartDate.Value >= targetStart &&
                           c.ScheduledStartDate.Value <= targetEnd)
                .ToList();

            if (!upcomingChanges.Any())
            {
                _logger.LogInformation("✅ No changes need 1-hour reminders");
                return;
            }

            _logger.LogInformation("📬 Sending 1-hour reminders for {Count} change(s)", upcomingChanges.Count);

            foreach (var change in upcomingChanges)
            {
                await SendReminderEmail(change, 1);
            }

            _logger.LogInformation("✅ Completed 1-hour reminders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SendReminders1HourBefore job");
            throw;
        }
    }

    private async Task SendReminderEmail(ChangeRequest change, int hoursUntilStart)
    {
        try
        {
            // Get all users in tenant
            var allUsers = await _userRepository.ListAsync();
            var tenantUsers = allUsers
                .Where(u => u.TenantId == change.TenantId && !string.IsNullOrEmpty(u.Email))
                .Select(u => (Email: u.Email!, Name: $"{u.FirstName} {u.LastName}"))
                .ToList();

            if (!tenantUsers.Any())
            {
                _logger.LogWarning("No users with email found for tenant {TenantId}", change.TenantId);
                return;
            }

            // Create email model
            var emailModel = new ChangeReminderEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                ChangeRequestId = change.Id.Value.ToString(),
                Title = change.Title,
                ScheduledStartDate = change.ScheduledStartDate!.Value,
                ChangeWindow = change.ChangeWindow ?? "Not specified",
                HoursUntilStart = hoursUntilStart,
                AffectedSystems = change.AffectedSystems,
                RollbackPlan = change.RollbackPlan
            };

            var subject = hoursUntilStart == 24
                ? $"⏰ [24h Reminder] Change Starting Tomorrow: {change.Title}"
                : $"🚨 [1h Reminder] Change Starting Soon: {change.Title}";

            var htmlBody = BuildReminderEmailHtml(emailModel, hoursUntilStart);

            await _emailService.SendEmailAsync(
                tenantUsers,
                subject,
                htmlBody);

            _logger.LogInformation(
                "📧 Sent {Hours}h reminder for change {Id} to {Count} recipients",
                hoursUntilStart,
                change.Id,
                tenantUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Error sending reminder email for change {Id}",
                change.Id);
        }
    }

    private static string BuildReminderEmailHtml(ChangeReminderEmail model, int hours)
    {
        var urgencyColor = hours == 1 ? "#dc2626" : "#ea580c";
        var urgencyEmoji = hours == 1 ? "🚨" : "⏰";

        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: {urgencyColor};">{urgencyEmoji} Change Reminder - {hours} Hour(s) Until Start</h2>
                    
                    <div style="background: #fef3c7; border-left: 4px solid {urgencyColor}; padding: 15px; margin: 20px 0;">
                        <p style="margin: 0; font-weight: bold;">
                            This change is scheduled to start in {hours} hour{(hours > 1 ? "s" : "")}!
                        </p>
                    </div>
                    
                    <div style="background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #1f2937;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Scheduled Start:</strong> {model.ScheduledStartDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Change Window:</strong> {model.ChangeWindow}</p>
                        <p><strong>Affected Systems:</strong> {model.AffectedSystems}</p>
                    </div>
                    
                    <div style="margin: 20px 0;">
                        <h4>Rollback Plan:</h4>
                        <p>{model.RollbackPlan}</p>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ViewUrl}" 
                           style="background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                            View Change Details →
                        </a>
                    </div>
                    
                    <p style="color: #6b7280; font-size: 14px; margin-top: 30px;">
                        This is an automated reminder from APEX Platform.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
