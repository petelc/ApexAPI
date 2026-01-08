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
/// Sends email notification when change request is completed successfully
/// </summary>
public class SendEmailOnChangeCompletedHandler : INotificationHandler<ChangeRequestCompletedEvent>
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailOnChangeCompletedHandler> _logger;
    private readonly EmailOptions _emailOptions;

    public SendEmailOnChangeCompletedHandler(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<SendEmailOnChangeCompletedHandler> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ChangeRequestCompletedEvent notification, CancellationToken cancellationToken)
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

            // Calculate duration
            TimeSpan? duration = null;
            if (changeRequest.ActualStartDate.HasValue && changeRequest.ActualEndDate.HasValue)
            {
                duration = changeRequest.ActualEndDate.Value - changeRequest.ActualStartDate.Value;
            }

            // Create email model
            var emailModel = new ChangeCompletedEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                ChangeRequestId = changeRequest.Id.Value.ToString(),
                Title = changeRequest.Title,
                CompletedDate = changeRequest.CompletedDate ?? DateTime.UtcNow,
                ActualStartDate = changeRequest.ActualStartDate ?? DateTime.UtcNow,
                ActualEndDate = changeRequest.ActualEndDate ?? DateTime.UtcNow,
                ImplementationNotes = changeRequest.ImplementationNotes
            };

            var subject = $"✅ [COMPLETED] Change Request: {changeRequest.Title}";
            var htmlBody = BuildCompletedEmailHtml(emailModel, duration);

            await _emailService.SendEmailAsync(
                tenantUsers,
                subject,
                htmlBody,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent completion email for ChangeRequest {Id} to {Count} recipients",
                notification.ChangeRequestId,
                tenantUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending completion email for ChangeRequest {Id}", notification.ChangeRequestId);
            // Don't throw - email failures shouldn't break the workflow
        }
    }

    private static string BuildCompletedEmailHtml(ChangeCompletedEmail model, TimeSpan? duration)
    {
        var durationText = duration.HasValue
            ? $"{duration.Value.Hours}h {duration.Value.Minutes}m"
            : "Unknown";

        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #059669;">✅ Change Request Completed Successfully</h2>
                    
                    <p>Great news! The following change has been completed successfully.</p>
                    
                    <div style="background: #d1fae5; border-left: 4px solid #059669; padding: 20px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #065f46;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Completed:</strong> {model.CompletedDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Started:</strong> {model.ActualStartDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Ended:</strong> {model.ActualEndDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Duration:</strong> {durationText}</p>
                    </div>
                    
                    {(string.IsNullOrEmpty(model.ImplementationNotes) ? "" : $"""
                    <div style="margin: 20px 0;">
                        <h4>Implementation Notes:</h4>
                        <p style="background: #f3f4f6; padding: 15px; border-radius: 6px;">
                            {model.ImplementationNotes}
                        </p>
                    </div>
                    """)}
                    
                    <div style="background: #dbeafe; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">📋 Post-Implementation:</h4>
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            <li>Verify all affected systems are functioning correctly</li>
                            <li>Monitor for any unexpected behavior</li>
                            <li>Document any lessons learned</li>
                            <li>Close any related tickets or tasks</li>
                        </ul>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ViewUrl}" 
                           style="background: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
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
