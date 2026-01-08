using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Aggregates.ChangeRequestAggregate.Events;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Infrastructure.Email;
using Apex.API.Infrastructure.Email.Templates;

namespace Apex.API.Infrastructure.Email.EventHandlers;

/// <summary>
/// Sends email notification when change request is scheduled
/// </summary>
public class SendEmailOnChangeScheduledHandler : INotificationHandler<ChangeRequestScheduledEvent>
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailOnChangeScheduledHandler> _logger;
    private readonly EmailOptions _emailOptions;

    public SendEmailOnChangeScheduledHandler(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<SendEmailOnChangeScheduledHandler> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ChangeRequestScheduledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get change request details
            var changeRequest = await _changeRequestRepository.GetByIdAsync(notification.ChangeRequestId, cancellationToken);
            if (changeRequest == null)
            {
                _logger.LogWarning("ChangeRequest not found: {ChangeRequestId}", notification.ChangeRequestId);
                return;
            }

            // Get all users in tenant
            var allUsers = await _userRepository.ListAsync(cancellationToken);
            var tenantUsers = allUsers
                .Where(u => u.TenantId == changeRequest.TenantId && !string.IsNullOrEmpty(u.Email))
                .Select(u => (Email: u.Email!, Name: $"{u.FirstName} {u.LastName}"))
                .ToList();

            if (!tenantUsers.Any())
            {
                _logger.LogWarning("No users with email found for tenant {TenantId}", changeRequest.TenantId);
                return;
            }

            // Create email model
            var emailModel = new ChangeScheduledEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                ChangeRequestId = changeRequest.Id.Value.ToString(),
                Title = changeRequest.Title,
                ScheduledStartDate = changeRequest.ScheduledStartDate!.Value,
                ScheduledEndDate = changeRequest.ScheduledEndDate!.Value,
                ChangeWindow = changeRequest.ChangeWindow ?? "Not specified",
                AffectedSystems = changeRequest.AffectedSystems
            };

            var subject = $"📅 [SCHEDULED] Change Request: {changeRequest.Title}";
            var htmlBody = BuildScheduledEmailHtml(emailModel);

            await _emailService.SendEmailAsync(
                tenantUsers,
                subject,
                htmlBody,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent scheduling email for ChangeRequest {Id} to {Count} recipients",
                notification.ChangeRequestId,
                tenantUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending scheduling email for ChangeRequest {Id}", notification.ChangeRequestId);
            // Don't throw - email failures shouldn't break the workflow
        }
    }

    private static string BuildScheduledEmailHtml(ChangeScheduledEmail model)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #2563eb;">📅 Change Request Scheduled</h2>
                    
                    <p>A change has been scheduled for execution. Please review the details and prepare accordingly.</p>
                    
                    <div style="background: #dbeafe; border-left: 4px solid #2563eb; padding: 20px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #1e40af;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Scheduled Start:</strong> <span style="color: #dc2626; font-weight: bold;">{model.ScheduledStartDate:MMM dd, yyyy HH:mm} UTC</span></p>
                        <p><strong>Scheduled End:</strong> {model.ScheduledEndDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Change Window:</strong> {model.ChangeWindow}</p>
                        <p><strong>Affected Systems:</strong> {model.AffectedSystems}</p>
                    </div>
                    
                    <div style="background: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">⚠️ Important Reminders:</h4>
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            <li>Review implementation plan before execution</li>
                            <li>Ensure rollback procedures are ready</li>
                            <li>Coordinate with affected teams</li>
                            <li>Monitor systems during change window</li>
                            <li>You will receive reminders 24h and 1h before execution</li>
                        </ul>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ViewUrl}" 
                           style="background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                            View Change Details →
                        </a>
                    </div>
                    
                    <p style="color: #6b7280; font-size: 14px; margin-top: 30px;">
                        This is an automated notification from APEX Platform.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
