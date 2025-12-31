using MediatR;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Aggregates.RequestAggregate.Events;

namespace Apex.API.UseCases.Requests.EventHandlers;

/// <summary>
/// Handles RequestCreatedEvent - logs and can send notifications
/// </summary>
public class RequestCreatedEventHandler : INotificationHandler<RequestCreatedEvent>
{
    private readonly ILogger<RequestCreatedEventHandler> _logger;

    public RequestCreatedEventHandler(ILogger<RequestCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(RequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🎉 NEW REQUEST CREATED! RequestId: {RequestId}, Title: {Title}, CreatedBy: {CreatedBy}",
            notification.RequestId,
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
