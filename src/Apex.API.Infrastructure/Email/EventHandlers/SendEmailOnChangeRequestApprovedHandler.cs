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
/// Sends email notification when change request is approved
/// </summary>
public class SendEmailOnChangeRequestApprovedHandler : INotificationHandler<ChangeRequestApprovedEvent>
{
    private readonly IReadRepository<ChangeRequest> _changeRequestRepository;
    private readonly IReadRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailOnChangeRequestApprovedHandler> _logger;
    private readonly EmailOptions _emailOptions;

    public SendEmailOnChangeRequestApprovedHandler(
        IReadRepository<ChangeRequest> changeRequestRepository,
        IReadRepository<User> userRepository,
        IEmailService emailService,
        ILogger<SendEmailOnChangeRequestApprovedHandler> logger,
        IOptions<EmailOptions> emailOptions)
    {
        _changeRequestRepository = changeRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _emailOptions = emailOptions.Value;
    }

    public async Task Handle(ChangeRequestApprovedEvent notification, CancellationToken cancellationToken)
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

            // Get approver
            var approver = allUsers.FirstOrDefault(u => u.Id == changeRequest.ApprovedByUserId);
            var approverName = approver != null 
                ? $"{approver.FirstName} {approver.LastName}" 
                : "CAB";

            // Create email model
            var emailModel = new ChangeRequestApprovedEmail
            {
                BaseUrl = _emailOptions.BaseUrl,
                RecipientName = $"{submitter.FirstName} {submitter.LastName}",
                ChangeRequestId = changeRequest.Id.Value.ToString(),
                Title = changeRequest.Title,
                ApprovedBy = approverName,
                ApprovedDate = changeRequest.ApprovedDate ?? DateTime.UtcNow,
                ApprovalNotes = changeRequest.ApprovalNotes
            };

            var subject = $"✅ [APPROVED] Your Change Request: {changeRequest.Title}";
            var htmlBody = BuildApprovedEmailHtml(emailModel);

            await _emailService.SendEmailAsync(
                submitter.Email,
                $"{submitter.FirstName} {submitter.LastName}",
                subject,
                htmlBody,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Sent approval email for ChangeRequest {Id} to {Email}",
                notification.ChangeRequestId,
                submitter.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending approval email for ChangeRequest {Id}", notification.ChangeRequestId);
            // Don't throw - email failures shouldn't break the workflow
        }
    }

    private static string BuildApprovedEmailHtml(ChangeRequestApprovedEmail model)
    {
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h2 style="color: #059669;">✅ Change Request Approved</h2>
                    
                    <p>Hi {model.RecipientName},</p>
                    
                    <p>Good news! Your change request has been approved by the Change Advisory Board.</p>
                    
                    <div style="background: #d1fae5; border-left: 4px solid #059669; padding: 20px; margin: 20px 0;">
                        <h3 style="margin-top: 0; color: #065f46;">{model.Title}</h3>
                        <p><strong>Change ID:</strong> {model.ChangeRequestId}</p>
                        <p><strong>Approved By:</strong> {model.ApprovedBy}</p>
                        <p><strong>Approved Date:</strong> {model.ApprovedDate:MMM dd, yyyy HH:mm} UTC</p>
                    </div>
                    
                    {(string.IsNullOrEmpty(model.ApprovalNotes) ? "" : $"""
                    <div style="margin: 20px 0;">
                        <h4>Approval Notes:</h4>
                        <p style="background: #f3f4f6; padding: 15px; border-radius: 6px;">{model.ApprovalNotes}</p>
                    </div>
                    """)}
                    
                    <div style="background: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;">
                        <h4 style="margin-top: 0;">📋 Next Steps:</h4>
                        <ol style="margin: 10px 0; padding-left: 20px;">
                            <li>Schedule the change execution window</li>
                            <li>Coordinate with your team</li>
                            <li>Prepare implementation plan</li>
                            <li>Review rollback procedures</li>
                        </ol>
                    </div>
                    
                    <div style="margin: 30px 0;">
                        <a href="{model.ViewUrl}" 
                           style="background: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
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
