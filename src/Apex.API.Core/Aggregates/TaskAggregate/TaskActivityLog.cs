using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.TaskAggregate;

/// <summary>
/// Activity log entry - tracks all actions performed on a task
/// Provides audit trail and timeline for the task
/// </summary>
public class TaskActivityLog : EntityBase
{
    // Strong-typed IDs
    private TaskActivityLogId _id;
    public new TaskActivityLogId Id
    {
        get => _id;
        private set => _id = value;
    }

    private TaskId _taskId;
    public TaskId TaskId
    {
        get => _taskId;
        private set => _taskId = value;
    }

    // Activity Information
    public TaskActivityType ActivityType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Details { get; private set; }  // JSON or additional context

    // User Tracking
    public Guid UserId { get; private set; }
    public DateTime Timestamp { get; private set; }

    // Navigation
    public Task Task { get; private set; } = null!;

    // EF Core constructor
    private TaskActivityLog() 
    { 
        ActivityType = TaskActivityType.Created;  // Default value for EF Core
    }

    /// <summary>
    /// Creates a new activity log entry (factory method)
    /// </summary>
    public static TaskActivityLog Create(
        TaskId taskId,
        TaskActivityType activityType,
        string description,
        Guid userId,
        string? details = null)
    {
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

        if (details != null && details.Length > 5000)
            throw new ArgumentException("Details cannot exceed 5000 characters", nameof(details));

        var log = new TaskActivityLog
        {
            Id = TaskActivityLogId.CreateUnique(),
            TaskId = taskId,
            ActivityType = activityType,
            Description = description,
            Details = details,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        return log;
    }

    /// <summary>
    /// Factory methods for common activities
    /// </summary>
    public static TaskActivityLog Created(TaskId taskId, Guid userId, string title)
        => Create(taskId, TaskActivityType.Created, $"Task created: {title}", userId);

    public static TaskActivityLog Updated(TaskId taskId, Guid userId, string changes)
        => Create(taskId, TaskActivityType.Updated, "Task details updated", userId, changes);

    public static TaskActivityLog AssignedToUser(TaskId taskId, Guid userId, Guid assignedToUserId)
        => Create(taskId, TaskActivityType.Assigned, $"Assigned to user", userId, $"AssignedToUserId: {assignedToUserId}");

    public static TaskActivityLog AssignedToDepartment(TaskId taskId, Guid userId, string departmentName)
        => Create(taskId, TaskActivityType.Assigned, $"Assigned to department: {departmentName}", userId);

    public static TaskActivityLog Claimed(TaskId taskId, Guid userId)
        => Create(taskId, TaskActivityType.Claimed, "Task claimed", userId);

    public static TaskActivityLog Started(TaskId taskId, Guid userId)
        => Create(taskId, TaskActivityType.Started, "Task started", userId);

    public static TaskActivityLog Blocked(TaskId taskId, Guid userId, string reason)
        => Create(taskId, TaskActivityType.Blocked, "Task blocked", userId, reason);

    public static TaskActivityLog Unblocked(TaskId taskId, Guid userId)
        => Create(taskId, TaskActivityType.Unblocked, "Task unblocked", userId);

    public static TaskActivityLog Completed(TaskId taskId, Guid userId, string? resolutionNotes)
        => Create(taskId, TaskActivityType.Completed, "Task completed", userId, resolutionNotes);

    public static TaskActivityLog Cancelled(TaskId taskId, Guid userId, string reason)
        => Create(taskId, TaskActivityType.Cancelled, "Task cancelled", userId, reason);

    public static TaskActivityLog TimeLogged(TaskId taskId, Guid userId, decimal hours)
        => Create(taskId, TaskActivityType.TimeLogged, $"Logged {hours} hours", userId);

    public static TaskActivityLog CommentAdded(TaskId taskId, Guid userId, string comment)
        => Create(taskId, TaskActivityType.CommentAdded, "Comment added", userId, comment);

    public static TaskActivityLog ChecklistItemAdded(TaskId taskId, Guid userId, string itemDescription)
        => Create(taskId, TaskActivityType.ChecklistItemAdded, "Checklist item added", userId, itemDescription);

    public static TaskActivityLog ChecklistItemCompleted(TaskId taskId, Guid userId, string itemDescription)
        => Create(taskId, TaskActivityType.ChecklistItemCompleted, "Checklist item completed", userId, itemDescription);

    public static TaskActivityLog NotesUpdated(TaskId taskId, Guid userId, string noteType)
        => Create(taskId, TaskActivityType.NotesUpdated, $"{noteType} updated", userId);

    public override string ToString()
    {
        return $"{Timestamp:g}: {ActivityType.Name} - {Description}";
    }
}
