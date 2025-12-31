using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.ProjectRequestAggregate.Events;

/// <summary>
/// Event raised when a new project request is created
/// </summary>
public class ProjectRequestCreatedEvent : DomainEventBase
{
    public ProjectRequestId ProjectRequestId { get; }
    public string Title { get; }
    public Guid CreatedByUserId { get; }

    public ProjectRequestCreatedEvent(ProjectRequestId projectRequestId, string title, Guid createdByUserId)
    {
        ProjectRequestId = projectRequestId;
        Title = title;
        CreatedByUserId = createdByUserId;
    }
}

/// <summary>
/// Event raised when a project request is submitted for approval
/// </summary>
public class ProjectRequestSubmittedEvent : DomainEventBase
{
    public ProjectRequestId ProjectRequestId { get; }
    public Guid SubmittedByUserId { get; }

    public ProjectRequestSubmittedEvent(ProjectRequestId projectRequestId, Guid submittedByUserId)
    {
        ProjectRequestId = projectRequestId;
        SubmittedByUserId = submittedByUserId;
    }
}

/// <summary>
/// Event raised when a project request is approved
/// </summary>
public class ProjectRequestApprovedEvent : DomainEventBase
{
    public ProjectRequestId ProjectRequestId { get; }
    public Guid ApprovedByUserId { get; }
    public string? ApprovalNotes { get; }

    public ProjectRequestApprovedEvent(ProjectRequestId projectRequestId, Guid approvedByUserId, string? approvalNotes)
    {
        ProjectRequestId = projectRequestId;
        ApprovedByUserId = approvedByUserId;
        ApprovalNotes = approvalNotes;
    }
}

/// <summary>
/// Event raised when a project request is rejected/denied
/// </summary>
public class ProjectRequestRejectedEvent : DomainEventBase
{
    public ProjectRequestId ProjectRequestId { get; }
    public Guid RejectedByUserId { get; }
    public string RejectionReason { get; }

    public ProjectRequestRejectedEvent(ProjectRequestId projectRequestId, Guid rejectedByUserId, string rejectionReason)
    {
        ProjectRequestId = projectRequestId;
        RejectedByUserId = rejectedByUserId;
        RejectionReason = rejectionReason;
    }
}

/// <summary>
/// Event raised when a project request is converted to a project
/// </summary>
public class ProjectRequestConvertedEvent : DomainEventBase
{
    public ProjectRequestId ProjectRequestId { get; }
    public Guid ProjectId { get; }
    public Guid ConvertedByUserId { get; }

    public ProjectRequestConvertedEvent(ProjectRequestId projectRequestId, Guid projectId, Guid convertedByUserId)
    {
        ProjectRequestId = projectRequestId;
        ProjectId = projectId;
        ConvertedByUserId = convertedByUserId;
    }
}
