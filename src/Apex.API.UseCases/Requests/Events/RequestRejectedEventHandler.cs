using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.RequestAggregate.Events;

namespace Apex.API.UseCases.Requests.EventHandlers;

/// <summary>
/// Handles RequestRejectedEvent - notifies requester with reason
/// </summary>
public class RequestRejectedEventHandler : INotificationHandler<RequestRejectedEvent>
{
    private readonly ILogger<RequestRejectedEventHandler> _logger;

    public RequestRejectedEventHandler(ILogger<RequestRejectedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(RequestRejectedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "❌ REQUEST DENIED! RequestId: {RequestId}, RejectedBy: {RejectedBy}, Reason: {Reason}",
            notification.RequestId,
            notification.RejectedByUserId,
            notification.RejectionReason);

        // TODO: Add real functionality:
        // - Send email to requester with denial reason
        // - Provide feedback on how to improve
        // - Update rejection metrics
        // - Log in audit trail
        // - Archive or mark for follow-up
        // - Suggest alternative approaches

        return Task.CompletedTask;
    }
}
