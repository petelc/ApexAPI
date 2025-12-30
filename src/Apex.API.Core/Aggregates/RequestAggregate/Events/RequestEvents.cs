using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.RequestAggregate.Events;

/// <summary>
/// Event raised when a new request is created
/// </summary>
public class RequestCreatedEvent : DomainEventBase
{
    public RequestId RequestId { get; }
    public string Title { get; }
    public Guid CreatedByUserId { get; }

    public RequestCreatedEvent(RequestId requestId, string title, Guid createdByUserId)
    {
        RequestId = requestId;
        Title = title;
        CreatedByUserId = createdByUserId;
    }
}

/// <summary>
/// Event raised when a request is submitted for approval
/// </summary>
public class RequestSubmittedEvent : DomainEventBase
{
    public RequestId RequestId { get; }
    public Guid SubmittedByUserId { get; }

    public RequestSubmittedEvent(RequestId requestId, Guid submittedByUserId)
    {
        RequestId = requestId;
        SubmittedByUserId = submittedByUserId;
    }
}

/// <summary>
/// Event raised when a request is approved
/// </summary>
public class RequestApprovedEvent : DomainEventBase
{
    public RequestId RequestId { get; }
    public Guid ApprovedByUserId { get; }
    public string? ApprovalNotes { get; }

    public RequestApprovedEvent(RequestId requestId, Guid approvedByUserId, string? approvalNotes)
    {
        RequestId = requestId;
        ApprovedByUserId = approvedByUserId;
        ApprovalNotes = approvalNotes;
    }
}

/// <summary>
/// Event raised when a request is rejected/denied
/// </summary>
public class RequestRejectedEvent : DomainEventBase
{
    public RequestId RequestId { get; }
    public Guid RejectedByUserId { get; }
    public string RejectionReason { get; }

    public RequestRejectedEvent(RequestId requestId, Guid rejectedByUserId, string rejectionReason)
    {
        RequestId = requestId;
        RejectedByUserId = rejectedByUserId;
        RejectionReason = rejectionReason;
    }
}

/// <summary>
/// Event raised when a request is completed
/// </summary>
public class RequestCompletedEvent : DomainEventBase
{
    public RequestId RequestId { get; }
    public Guid CompletedByUserId { get; }

    public RequestCompletedEvent(RequestId requestId, Guid completedByUserId)
    {
        RequestId = requestId;
        CompletedByUserId = completedByUserId;
    }
}

/// <summary>
/// Event raised when a request is assigned to a user
/// </summary>
public class RequestAssignedEvent : DomainEventBase
{
    public RequestId RequestId { get; }
    public Guid AssignedToUserId { get; }
    public Guid AssignedByUserId { get; }

    public RequestAssignedEvent(RequestId requestId, Guid assignedToUserId, Guid assignedByUserId)
    {
        RequestId = requestId;
        AssignedToUserId = assignedToUserId;
        AssignedByUserId = assignedByUserId;
    }
}