using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.Infrastructure.Email;

namespace Apex.API.Infrastructure.Jobs;

/// <summary>
/// Background job to monitor and alert on overdue changes
/// </summary>
public class OverdueChangesJob
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OverdueChangesJob> _logger;
    private readonly EmailOptions _emailOptions;

    public OverdueChangesJob(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<OverdueChangesJob> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    /// <summary>
    /// Check for changes that are past their scheduled end time
    /// Runs every hour
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task CheckOverdueChanges()
    {
        _logger.LogInformation("🔍 Checking for overdue changes...");

        try
        {
            var now = DateTime.UtcNow;

            var allChanges = await _changeRequestRepository.ListAsync();

            // Find changes that are overdue (in progress or scheduled, past end time)
            var overdueChanges = allChanges
                .Where(c => (c.Status == ChangeRequestStatus.InProgress || 
                            c.Status == ChangeRequestStatus.Scheduled) &&
                           c.ScheduledEndDate.HasValue &&
                           c.ScheduledEndDate.Value < now)
                .ToList();

            if (!overdueChanges.Any())
            {
                _logger.LogInformation("✅ No overdue changes found");
                return;
            }

            _logger.LogWarning("⚠️ Found {Count} overdue change(s)", overdueChanges.Count);

            foreach (var change in overdueChanges)
            {
                await SendOverdueAlert(change);
            }

            _logger.LogInformation("✅ Completed overdue change checks");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in CheckOverdueChanges job");
            throw;
        }
    }

    private async Task SendOverdueAlert(ChangeRequest change)
    {
        try
        {
            // Get all users in tenant (send to operations team)
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

            var hoursOverdue = (DateTime.UtcNow - change.ScheduledEndDate!.Value).TotalHours;

            var subject = $"⚠️ [OVERDUE] Change Past Scheduled End Time: {change.Title}";
            var htmlBody = BuildOverdueEmailHtml(change, hoursOverdue);

            await _emailService.SendEmailAsync(
                tenantUsers,
                subject,
                htmlBody);

            _logger.LogWarning(
                "⚠️ Sent overdue alert for change {Id} ({Hours:F1}h overdue) to {Count} recipients",
                change.Id,
                hoursOverdue,
                tenantUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Error sending overdue alert for change {Id}",
                change.Id);
        }
    }

    private string BuildOverdueEmailHtml(ChangeRequest change, double hoursOverdue)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #dc2626;">⚠️ Change Request Overdue</h2>
                    
                    <div style="background: #fee2e2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;">
                        <p style="margin: 0; font-weight: bold; color: #991b1b;">
                            This change is {hoursOverdue:F1} hours past its scheduled end time!
                        </p>
                    </div>
                    
                    <div style="background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #1f2937;">{change.Title}</h3>
                        <p><strong>Change ID:</strong> {change.Id.Value}</p>
                        <p><strong>Status:</strong> <span style="color: #dc2626;">{change.Status.Name}</span></p>
                        <p><strong>Scheduled End:</strong> {change.ScheduledEndDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Hours Overdue:</strong> {hoursOverdue:F1}</p>
                        <p><strong>Affected Systems:</strong> {change.AffectedSystems}</p>
                    </div>
                    
                    <div style="background: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">⚡ Action Required:</h4>
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            <li>Check change execution status</li>
                            <li>Determine if completion is near or if issues occurred</li>
                            <li>Consider rollback if change has failed</li>
                            <li>Update change status in system</li>
                        </ul>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{_emailOptions.BaseUrl}/change-requests/{change.Id.Value}" 
                           style="background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                            View Change Details →
                        </a>
                    </div>
                    
                    <p style="color: #6b7280; font-size: 14px; margin-top: 30px;">
                        This is an automated alert from APEX Platform.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
