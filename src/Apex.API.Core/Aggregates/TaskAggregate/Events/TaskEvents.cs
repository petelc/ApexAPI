using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.TaskAggregate.Events;

/// <summary>
/// Event raised when a new task is created
/// </summary>
public class TaskCreatedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public ProjectId ProjectId { get; }
    public string Title { get; }
    public Guid CreatedByUserId { get; }

    public TaskCreatedEvent(TaskId taskId, ProjectId projectId, string title, Guid createdByUserId)
    {
        TaskId = taskId;
        ProjectId = projectId;
        Title = title;
        CreatedByUserId = createdByUserId;
    }
}

/// <summary>
/// Event raised when a task is assigned to a specific user
/// </summary>
public class TaskAssignedToUserEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public Guid AssignedToUserId { get; }
    public Guid AssignedByUserId { get; }

    public TaskAssignedToUserEvent(TaskId taskId, Guid assignedToUserId, Guid assignedByUserId)
    {
        TaskId = taskId;
        AssignedToUserId = assignedToUserId;
        AssignedByUserId = assignedByUserId;
    }
}

/// <summary>
/// Event raised when a task is assigned to a department
/// </summary>
public class TaskAssignedToDepartmentEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public DepartmentId DepartmentId { get; }
    public Guid AssignedByUserId { get; }

    public TaskAssignedToDepartmentEvent(TaskId taskId, DepartmentId departmentId, Guid assignedByUserId)
    {
        TaskId = taskId;
        DepartmentId = departmentId;
        AssignedByUserId = assignedByUserId;
    }
}

/// <summary>
/// Event raised when a user claims a department-assigned task
/// </summary>
public class TaskClaimedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public Guid ClaimedByUserId { get; }
    public DepartmentId DepartmentId { get; }

    public TaskClaimedEvent(TaskId taskId, Guid claimedByUserId, DepartmentId departmentId)
    {
        TaskId = taskId;
        ClaimedByUserId = claimedByUserId;
        DepartmentId = departmentId;
    }
}

/// <summary>
/// Event raised when a task is started
/// </summary>
public class TaskStartedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public Guid StartedByUserId { get; }

    public TaskStartedEvent(TaskId taskId, Guid startedByUserId)
    {
        TaskId = taskId;
        StartedByUserId = startedByUserId;
    }
}

/// <summary>
/// Event raised when time is logged on a task
/// </summary>
public class TaskTimeLoggedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public decimal HoursLogged { get; }
    public decimal TotalActualHours { get; }

    public TaskTimeLoggedEvent(TaskId taskId, decimal hoursLogged, decimal totalActualHours)
    {
        TaskId = taskId;
        HoursLogged = hoursLogged;
        TotalActualHours = totalActualHours;
    }
}

/// <summary>
/// Event raised when a task is blocked
/// </summary>
public class TaskBlockedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public string Reason { get; }
    public Guid BlockedByUserId { get; }

    public TaskBlockedEvent(TaskId taskId, string reason, Guid blockedByUserId)
    {
        TaskId = taskId;
        Reason = reason;
        BlockedByUserId = blockedByUserId;
    }
}

/// <summary>
/// Event raised when a task is unblocked
/// </summary>
public class TaskUnblockedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public Guid UnblockedByUserId { get; }

    public TaskUnblockedEvent(TaskId taskId, Guid unblockedByUserId)
    {
        TaskId = taskId;
        UnblockedByUserId = unblockedByUserId;
    }
}

/// <summary>
/// Event raised when a task is completed
/// </summary>
public class TaskCompletedEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public Guid CompletedByUserId { get; }

    public TaskCompletedEvent(TaskId taskId, Guid completedByUserId)
    {
        TaskId = taskId;
        CompletedByUserId = completedByUserId;
    }
}

/// <summary>
/// Event raised when a task is cancelled
/// </summary>
public class TaskCancelledEvent : DomainEventBase
{
    public TaskId TaskId { get; }
    public Guid CancelledByUserId { get; }
    public string Reason { get; }

    public TaskCancelledEvent(TaskId taskId, Guid cancelledByUserId, string reason)
    {
        TaskId = taskId;
        CancelledByUserId = cancelledByUserId;
        Reason = reason;
    }
}
