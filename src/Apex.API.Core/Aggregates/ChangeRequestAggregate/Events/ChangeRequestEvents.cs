using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.ChangeRequestAggregate.Events;

/// <summary>
/// Fired when a change request is created
/// </summary>
public class ChangeRequestCreatedEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }
    public ChangeRequestCreatedEvent(ChangeRequestId changeRequestId, string title)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
    }
}

/// <summary>
/// Fired when a change request is submitted for CAB review
/// </summary>
public class ChangeRequestSubmittedEvent : DomainEventBase
{

    public ChangeRequestId ChangeRequestId { get; }
    public bool RequiresCABApproval { get; }
    public string Title { get; }
    public ChangeRequestSubmittedEvent(ChangeRequestId changeRequestId, string title, bool requiresCABApproval)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
        RequiresCABApproval = requiresCABApproval;
    }
}

/// <summary>
/// Fired when a change request is approved
/// </summary>
public class ChangeRequestApprovedEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }
    public Guid ApprovedByUserId { get; }

    public ChangeRequestApprovedEvent(ChangeRequestId changeRequestId, string title, Guid approvedByUserId)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
        ApprovedByUserId = approvedByUserId;
    }
}

/// <summary>
/// Fired when a change request is denied
/// </summary>
public class ChangeRequestDeniedEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }
    public string Reason { get; }

    public ChangeRequestDeniedEvent(ChangeRequestId changeRequestId, string title, string reason)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
        Reason = reason;
    }
}
/// <summary>
/// Fired when a change request is scheduled
/// </summary>
public class ChangeRequestScheduledEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }
    public DateTime ScheduledStartDate { get; }
    public DateTime ScheduledEndDate { get; }

    public ChangeRequestScheduledEvent(ChangeRequestId changeRequestId, string title, DateTime scheduledStartDate, DateTime scheduledEndDate)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
    }
}

/// <summary>
/// Fired when change execution starts
/// </summary>
public class ChangeRequestStartedEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }

    public ChangeRequestStartedEvent(ChangeRequestId changeRequestId, string title)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
    }
}

/// <summary>
/// Fired when a change is completed successfully
/// </summary>
public class ChangeRequestCompletedEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }

    public ChangeRequestCompletedEvent(ChangeRequestId changeRequestId, string title)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
    }
}

/// <summary>
/// Fired when a change fails
/// </summary>
public class ChangeRequestFailedEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }
    public string Reason { get; }

    public ChangeRequestFailedEvent(ChangeRequestId changeRequestId, string title, string reason)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
        Reason = reason;
    }
}

/// <summary>
/// Fired when a change is rolled back
/// </summary>
public class ChangeRequestRolledBackEvent : DomainEventBase
{
    public ChangeRequestId ChangeRequestId { get; }
    public string Title { get; }
    public string Reason { get; }

    public ChangeRequestRolledBackEvent(ChangeRequestId changeRequestId, string title, string reason)
    {
        ChangeRequestId = changeRequestId;
        Title = title;
        Reason = reason;
    }
}