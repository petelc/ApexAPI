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
/// Sends email notification when change request is denied
/// </summary>
public class SendEmailOnChangeRequestDeniedHandler : INotificationHandler<ChangeRequestDeniedEvent>
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailOnChangeRequestDeniedHandler> _logger;
    private readonly EmailOptions _emailOptions;

    public SendEmailOnChangeRequestDeniedHandler(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<SendEmailOnChangeRequestDeniedHandler> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ChangeRequestDeniedEvent notification, CancellationToken cancellationToken)
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

            // Get all users
            var allUsers = await _userRepository.ListAsync(cancellationToken);

            // Get submitter (person who created the change)
            var submitter = allUsers.FirstOrDefault(u => u.Id == changeRequest.CreatedByUserId);
            if (submitter == null || string.IsNullOrEmpty(submitter.Email))
            {
                _logger.LogWarning("Submitter not found or has no email for change {Id}", changeRequest.Id);
                return;
            }

            // Get reviewer who denied
            var reviewer = allUsers.FirstOrDefault(u => u.Id == changeRequest.ReviewedByUserId);
            var reviewerName = reviewer != null 
                ? $"{reviewer.FirstName} {reviewer.LastName}" 
                : "CAB";

            // Create email model
            var emailModel = new ChangeRequestDeniedEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                RecipientName = $"{submitter.FirstName} {submitter.LastName}",
                ChangeRequestId = changeRequest.Id.Value.ToString(),
                Title = changeRequest.Title,
                DeniedBy = reviewerName,
                DeniedDate = changeRequest.DeniedDate ?? DateTime.UtcNow,
                DenialReason = changeRequest.DenialReason ?? "No reason provided"
            };

            var subject = $"❌ [DENIED] Your Change Request: {changeRequest.Title}";
            var htmlBody = BuildDeniedEmailHtml(emailModel);

            await _emailService.SendEmailAsync(
                submitter.Email,
                $"{submitter.FirstName} {submitter.LastName}",
                subject,
                htmlBody,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent denial email for ChangeRequest {Id} to {Email}",
                notification.ChangeRequestId,
                submitter.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending denial email for ChangeRequest {Id}", notification.ChangeRequestId);
            // Don't throw - email failures shouldn't break the workflow
        }
    }

    private static string BuildDeniedEmailHtml(ChangeRequestDeniedEmail model)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #dc2626;">❌ Change Request Denied</h2>
                    
                    <p>Hi {model.RecipientName},</p>
                    
                    <p>Your change request has been reviewed and was not approved at this time.</p>
                    
                    <div style="background: #fee2e2; border-left: 4px solid #dc2626; padding: 20px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #991b1b;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Denied By:</strong> {model.DeniedBy}</p>
                        <p><strong>Denied Date:</strong> {model.DeniedDate:MMM dd, yyyy HH:mm} UTC</p>
                    </div>
                    
                    <div style="margin: 20px 0;">
                        <h4>Reason for Denial:</h4>
                        <p style="background: #fef2f2; padding: 15px; border-radius: 6px; border-left: 3px solid #dc2626;">
                            {model.DenialReason}
                        </p>
                    </div>
                    
                    <div style="background: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">📋 What You Can Do:</h4>
                        <ul style="margin: 10px 0; padding-left: 20px;">
                            <li>Review the denial reason carefully</li>
                            <li>Address the concerns raised by the CAB</li>
                            <li>Update your change request with additional information</li>
                            <li>Resubmit for review when ready</li>
                            <li>Contact the CAB if you have questions</li>
                        </ul>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ViewUrl}" 
                           style="background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                            View Change Request →
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
