using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.ProjectRequestAggregate.Events;

namespace Apex.API.UseCases.ProjectRequests.EventHandlers;

/// <summary>
/// Handles ProjectRequestRejectedEvent - notifies requester with reason
/// </summary>
public class ProjectRequestRejectedEventHandler : INotificationHandler<ProjectRequestRejectedEvent>
{
    private readonly ILogger<ProjectRequestRejectedEventHandler> _logger;

    public ProjectRequestRejectedEventHandler(ILogger<ProjectRequestRejectedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectRequestRejectedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "❌ ProjectRequest DENIED! ProjectRequestId: {ProjectRequestId}, RejectedBy: {RejectedBy}, Reason: {Reason}",
            notification.ProjectRequestId,
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
