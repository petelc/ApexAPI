using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.ProjectRequestAggregate.Events;

namespace Apex.API.UseCases.ProjectRequests.EventHandlers;

/// <summary>
/// Handles ProjectRequestApprovedEvent - notifies requester and prepares for assignment
/// </summary>
public class ProjectRequestApprovedEventHandler : INotificationHandler<ProjectRequestApprovedEvent>
{
    private readonly ILogger<ProjectRequestApprovedEventHandler> _logger;

    public ProjectRequestApprovedEventHandler(ILogger<ProjectRequestApprovedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectRequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✅ ProjectRequest APPROVED! ProjectRequestId: {ProjectRequestId}, ApprovedBy: {ApprovedBy}, Notes: {Notes}",
            notification.ProjectRequestId,
            notification.ApprovedByUserId,
            notification.ApprovalNotes ?? "(no notes)");

        // TODO: Add real functionality:
        // - Send email to requester (good news!)
        // - Notify team that ProjectRequest is ready for assignment
        // - Update approval metrics/dashboard
        // - Create tasks if auto-task-creation is enabled
        // - Add to "ready for work" queue
        // - Log approval in audit trail
        // - Trigger next workflow step

        return Task.CompletedTask;
    }
}
