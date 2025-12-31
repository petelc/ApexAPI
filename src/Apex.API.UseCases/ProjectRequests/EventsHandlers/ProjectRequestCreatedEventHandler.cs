using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.ProjectRequestAggregate.Events;

namespace Apex.API.UseCases.ProjectRequests.EventHandlers;

/// <summary>
/// Handles ProjectRequestCreatedEvent - logs and can send notifications
/// </summary>
public class ProjectRequestCreatedEventHandler : INotificationHandler<ProjectRequestCreatedEvent>
{
    private readonly ILogger<ProjectRequestCreatedEventHandler> _logger;

    public ProjectRequestCreatedEventHandler(ILogger<ProjectRequestCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🎉 NEW ProjectRequest CREATED! ProjectRequestId: {ProjectRequestId}, Title: {Title}, CreatedBy: {CreatedBy}",
            notification.ProjectRequestId,
            notification.Title,
            notification.CreatedByUserId);

        // TODO: Add real functionality:
        // - Send email to requester confirming creation
        // - Update dashboard/metrics
        // - Notify team members
        // - Create audit log entry
        // - Trigger workflows

        return Task.CompletedTask;
    }
}
