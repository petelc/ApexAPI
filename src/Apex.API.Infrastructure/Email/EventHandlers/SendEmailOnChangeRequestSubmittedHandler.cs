using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Aggregates.ChangeRequestAggregate.Events;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Infrastructure.Email.Templates;

namespace Apex.API.Infrastructure.Email.EventHandlers;

/// <summary>
/// Sends email notification when change request is submitted for CAB review
/// </summary>
public class SendEmailOnChangeRequestSubmittedHandler : INotificationHandler<ChangeRequestSubmittedEvent>
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailOnChangeRequestSubmittedHandler> _logger;
    private readonly EmailOptions _emailOptions;

    public SendEmailOnChangeRequestSubmittedHandler(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<SendEmailOnChangeRequestSubmittedHandler> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ChangeRequestSubmittedEvent notification, CancellationToken cancellationToken)
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

            // If doesn't require CAB approval, skip
            if (!notification.RequiresCABApproval)
            {
                _logger.LogInformation("ChangeRequest {Id} doesn't require CAB approval, skipping email",
                    notification.ChangeRequestId);
                return;
            }

            // Get all users
            var allUsers = await _userRepository.ListAsync(cancellationToken);

            // Get submitter details
            var submitter = allUsers.FirstOrDefault(u => u.Id == changeRequest.CreatedByUserId);

            // For now, send to all users in the same tenant with email addresses
            // TODO: Filter by CAB role once Role navigation property is available
            var cabMembers = allUsers
                .Where(u => u.TenantId == changeRequest.TenantId &&
                           !string.IsNullOrEmpty(u.Email))
                .Select(u => (u.Email!, $"{u.FirstName} {u.LastName}"))
                .ToList();

            if (!cabMembers.Any())
            {
                _logger.LogWarning("No users with email found for tenant {TenantId}", changeRequest.TenantId);
                return;
            }

            // Create email model
            var emailModel = new ChangeRequestSubmittedEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                ChangeRequestId = changeRequest.Id.Value.ToString(),
                Title = changeRequest.Title,
                Description = changeRequest.Description,
                ChangeType = changeRequest.ChangeType.Name,
                Priority = changeRequest.Priority.Name,
                RiskLevel = changeRequest.RiskLevel.Name,
                SubmittedBy = submitter != null ? $"{submitter.FirstName} {submitter.LastName}" : "Unknown",
                SubmittedDate = changeRequest.SubmittedDate ?? DateTime.UtcNow
            };

            // Build email HTML
            var subject = $"[CAB Review Required] {changeRequest.Title}";
            var htmlBody = BuildSubmittedEmailHtml(emailModel);

            // Send to all CAB members
            await _emailService.SendEmailAsync(
                cabMembers,
                subject,
                htmlBody,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent CAB review email for ChangeRequest {Id} to {Count} recipients",
                notification.ChangeRequestId,
                cabMembers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for ChangeRequestSubmitted event");
            // Don't throw - email failures shouldn't break the workflow
        }
    }

    private static string BuildSubmittedEmailHtml(ChangeRequestSubmittedEmail model)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #2563eb;">🔔 Change Request Submitted for CAB Review</h2>
                    
                    <p>A new change request has been submitted and requires CAB review:</p>
                    
                    <div style="background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #1f2937;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Type:</strong> <span style="color: #dc2626;">{model.ChangeType}</span></p>
                        <p><strong>Priority:</strong> <span style="color: #ea580c;">{model.Priority}</span></p>
                        <p><strong>Risk Level:</strong> <span style="color: #dc2626;">{model.RiskLevel}</span></p>
                        <p><strong>Submitted By:</strong> {model.SubmittedBy}</p>
                        <p><strong>Submitted:</strong> {model.SubmittedDate:MMM dd, yyyy HH:mm} UTC</p>
                    </div>
                    
                    <div style="margin: 20px 0;">
                        <h4>Description:</h4>
                        <p>{model.Description}</p>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ReviewUrl}" 
                           style="background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                            Review Change Request →
                        </a>
                    </div>
                    
                    <p style="color: #6b7280; font-size: 14px; margin-top: 30px;">
                        This is an automated notification from APEX Platform.<br>
                        <a href="{model.ViewUrl}" style="color: #2563eb;">View in APEX</a>
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
