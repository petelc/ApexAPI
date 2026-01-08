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
/// Sends email notification when change request is rolled back
/// </summary>
public class SendEmailOnChangeRolledBackHandler : INotificationHandler<ChangeRequestRolledBackEvent>
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailOnChangeRolledBackHandler> _logger;
    private readonly EmailOptions _emailOptions;

    public SendEmailOnChangeRolledBackHandler(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<SendEmailOnChangeRolledBackHandler> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ChangeRequestRolledBackEvent notification, CancellationToken cancellationToken)
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

            // Get all users in tenant - rollback is critical so notify everyone
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
            var emailModel = new ChangeRolledBackEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                ChangeRequestId = changeRequest.Id.Value.ToString(),
                Title = changeRequest.Title,
                RolledBackDate = changeRequest.RolledBackDate ?? DateTime.UtcNow,
                RollbackReason = changeRequest.RollbackReason ?? "Not specified",
                AffectedSystems = changeRequest.AffectedSystems
            };

            var subject = $"🚨 [ROLLBACK] Change Request Rolled Back: {changeRequest.Title}";
            var htmlBody = BuildRolledBackEmailHtml(emailModel);

            await _emailService.SendEmailAsync(
                tenantUsers,
                subject,
                htmlBody,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Sent rollback alert for ChangeRequest {Id} to {Count} recipients",
                notification.ChangeRequestId,
                tenantUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending rollback alert for ChangeRequest {Id}", notification.ChangeRequestId);
            // Don't throw - email failures shouldn't break the workflow
        }
    }

    private static string BuildRolledBackEmailHtml(ChangeRolledBackEmail model)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #dc2626;">🚨 Change Request Rolled Back</h2>
                    
                    <div style="background: #fee2e2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;">
                        <p style="margin: 0; font-weight: bold; color: #991b1b;">
                            CRITICAL: A change has been rolled back due to issues during implementation.
                        </p>
                    </div>
                    
                    <div style="background: #fef2f2; border-left: 4px solid #dc2626; padding: 20px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #991b1b;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Rolled Back:</strong> {model.RolledBackDate:MMM dd, yyyy HH:mm} UTC</p>
                        <p><strong>Affected Systems:</strong> {model.AffectedSystems}</p>
                    </div>
                    
                    <div style="margin: 20px 0;">
                        <h4>Rollback Reason:</h4>
                        <p style="background: #fee2e2; padding: 15px; border-radius: 6px; border-left: 3px solid #dc2626;">
                            {model.RollbackReason}
                        </p>
                    </div>
                    
                    <div style="background: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">⚠️ Immediate Actions Required:</h4>
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            <li><strong>Verify rollback completion</strong> - Ensure all systems are restored</li>
                            <li><strong>Monitor affected systems</strong> - Watch for any residual issues</li>
                            <li><strong>Notify stakeholders</strong> - Inform affected teams of the rollback</li>
                            <li><strong>Document the incident</strong> - Record what went wrong for post-mortem</li>
                            <li><strong>Schedule review meeting</strong> - Determine root cause and preventive measures</li>
                        </ul>
                    </div>
                    
                    <div style="background: #dbeafe; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">📋 Next Steps:</h4>
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            <li>Create incident report if necessary</li>
                            <li>Analyze failure cause</li>
                            <li>Update implementation plan to address issues</li>
                            <li>Resubmit change when ready (if applicable)</li>
                        </ul>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ViewUrl}" 
                           style="background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; margin-right: 10px;">
                            View Change Details →
                        </a>
                        <a href="{model.IncidentUrl}" 
                           style="background: #ea580c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                            Create Incident Report →
                        </a>
                    </div>
                    
                    <p style="color: #6b7280; font-size: 14px; margin-top: 30px;">
                        This is an automated critical alert from APEX Platform.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
