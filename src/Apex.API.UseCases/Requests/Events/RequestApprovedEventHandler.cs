using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.RequestAggregate.Events;

namespace Apex.API.UseCases.Requests.EventHandlers;

/// <summary>
/// Handles RequestApprovedEvent - notifies requester and prepares for assignment
/// </summary>
public class RequestApprovedEventHandler : INotificationHandler<RequestApprovedEvent>
{
    private readonly ILogger<RequestApprovedEventHandler> _logger;

    public RequestApprovedEventHandler(ILogger<RequestApprovedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(RequestApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✅ REQUEST APPROVED! RequestId: {RequestId}, ApprovedBy: {ApprovedBy}, Notes: {Notes}",
            notification.RequestId,
            notification.ApprovedByUserId,
            notification.ApprovalNotes ?? "(no notes)");

        // TODO: Add real functionality:
        // - Send email to requester (good news!)
        // - Notify team that request is ready for assignment
        // - Update approval metrics/dashboard
        // - Create tasks if auto-task-creation is enabled
        // - Add to "ready for work" queue
        // - Log approval in audit trail
        // - Trigger next workflow step

        return Task.CompletedTask;
    }
}
