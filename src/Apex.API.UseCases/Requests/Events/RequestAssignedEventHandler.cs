using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.RequestAggregate.Events;

namespace Apex.API.UseCases.Requests.EventHandlers;

/// <summary>
/// Handles RequestAssignedEvent - notifies assigned user
/// </summary>
public class RequestAssignedEventHandler : INotificationHandler<RequestAssignedEvent>
{
    private readonly ILogger<RequestAssignedEventHandler> _logger;

    public RequestAssignedEventHandler(ILogger<RequestAssignedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(RequestAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "👤 REQUEST ASSIGNED! RequestId: {RequestId}, AssignedTo: {AssignedTo}, AssignedBy: {AssignedBy}",
            notification.RequestId,
            notification.AssignedToUserId,
            notification.AssignedByUserId);

        // TODO: Add real functionality:
        // - Send email to assigned user
        // - Add to their work queue/dashboard
        // - Send Slack/Teams notification
        // - Start SLA timer for completion
        // - Update workload metrics
        // - Log assignment in audit trail
        // - Create calendar reminder

        return Task.CompletedTask;
    }
}
