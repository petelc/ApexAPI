using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.RequestAggregate.Events;

namespace Apex.API.UseCases.Requests.EventHandlers;

/// <summary>
/// Handles RequestSubmittedEvent - notifies reviewers
/// </summary>
public class RequestSubmittedEventHandler : INotificationHandler<RequestSubmittedEvent>
{
    private readonly ILogger<RequestSubmittedEventHandler> _logger;

    public RequestSubmittedEventHandler(ILogger<RequestSubmittedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(RequestSubmittedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "📋 REQUEST SUBMITTED FOR REVIEW! RequestId: {RequestId}, SubmittedBy: {SubmittedBy}",
            notification.RequestId,
            notification.SubmittedByUserId);

        // TODO: Add real functionality:
        // - Send email to CMB reviewers/managers
        // - Add to review queue
        // - Create Slack/Teams notification
        // - Update review dashboard
        // - Set SLA timer
        // - Log in audit trail

        return Task.CompletedTask;
    }
}
