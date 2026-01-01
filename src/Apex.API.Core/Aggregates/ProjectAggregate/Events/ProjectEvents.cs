using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.ProjectAggregate.Events;

/// <summary>
/// Event raised when a new project is created
/// </summary>
public class ProjectCreatedEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid ProjectRequestId { get; }
    public string ProjectName { get; }
    public Guid CreatedByUserId { get; }

    public ProjectCreatedEvent(ProjectId projectId, Guid projectRequestId, string projectName, Guid createdByUserId)
    {
        ProjectId = projectId;
        ProjectRequestId = projectRequestId;
        ProjectName = projectName;
        CreatedByUserId = createdByUserId;
    }
}

/// <summary>
/// Event raised when a project manager is assigned
/// </summary>
public class ProjectManagerAssignedEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid ProjectManagerUserId { get; }
    public Guid AssignedByUserId { get; }
    public Guid? PreviousProjectManagerUserId { get; }

    public ProjectManagerAssignedEvent(
        ProjectId projectId, 
        Guid projectManagerUserId, 
        Guid assignedByUserId,
        Guid? previousProjectManagerUserId)
    {
        ProjectId = projectId;
        ProjectManagerUserId = projectManagerUserId;
        AssignedByUserId = assignedByUserId;
        PreviousProjectManagerUserId = previousProjectManagerUserId;
    }
}

/// <summary>
/// Event raised when a project is started
/// </summary>
public class ProjectStartedEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid StartedByUserId { get; }

    public ProjectStartedEvent(ProjectId projectId, Guid startedByUserId)
    {
        ProjectId = projectId;
        StartedByUserId = startedByUserId;
    }
}

/// <summary>
/// Event raised when a project is put on hold
/// </summary>
public class ProjectPutOnHoldEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid UserId { get; }
    public string Reason { get; }

    public ProjectPutOnHoldEvent(ProjectId projectId, Guid userId, string reason)
    {
        ProjectId = projectId;
        UserId = userId;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a project is resumed from hold
/// </summary>
public class ProjectResumedEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid UserId { get; }

    public ProjectResumedEvent(ProjectId projectId, Guid userId)
    {
        ProjectId = projectId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a project is completed
/// </summary>
public class ProjectCompletedEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid CompletedByUserId { get; }

    public ProjectCompletedEvent(ProjectId projectId, Guid completedByUserId)
    {
        ProjectId = projectId;
        CompletedByUserId = completedByUserId;
    }
}

/// <summary>
/// Event raised when a project is cancelled
/// </summary>
public class ProjectCancelledEvent : DomainEventBase
{
    public ProjectId ProjectId { get; }
    public Guid CancelledByUserId { get; }
    public string Reason { get; }

    public ProjectCancelledEvent(ProjectId projectId, Guid cancelledByUserId, string reason)
    {
        ProjectId = projectId;
        CancelledByUserId = cancelledByUserId;
        Reason = reason;
    }
}
