using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.ProjectRequestAggregate.Events;

namespace Apex.API.UseCases.ProjectRequests.EventHandlers;

/// <summary>
/// Handles ProjectRequestSubmittedEvent - notifies reviewers
/// </summary>
public class ProjectRequestSubmittedEventHandler : INotificationHandler<ProjectRequestSubmittedEvent>
{
    private readonly ILogger<ProjectRequestSubmittedEventHandler> _logger;

    public ProjectRequestSubmittedEventHandler(ILogger<ProjectRequestSubmittedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectRequestSubmittedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "📋 ProjectRequest SUBMITTED FOR REVIEW! ProjectRequestId: {ProjectRequestId}, SubmittedBy: {SubmittedBy}",
            notification.ProjectRequestId,
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
