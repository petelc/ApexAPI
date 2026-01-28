using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.TaskAggregate;

/// <summary>
/// Checklist item entity - represents a to-do item within a task
/// Part of Task aggregate (not its own aggregate root)
/// </summary>
public class TaskChecklistItem : EntityBase
{
    // Strong-typed IDs
    private TaskChecklistItemId _id;
    public new TaskChecklistItemId Id
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

    // Core Information
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public int Order { get; private set; }

    // User Tracking
    public Guid? CompletedByUserId { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    
    public DateTime CreatedDate { get; private set; }

    // Navigation
    public Task Task { get; private set; } = null!;

    // EF Core constructor
    private TaskChecklistItem() { }

    /// <summary>
    /// Creates a new checklist item (factory method)
    /// </summary>
    public static TaskChecklistItem Create(
        TaskId taskId,
        string description,
        int order)
    {
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (description.Length < 2)
            throw new ArgumentException("Checklist item description must be at least 2 characters", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Checklist item description cannot exceed 500 characters", nameof(description));

        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        var item = new TaskChecklistItem
        {
            Id = TaskChecklistItemId.CreateUnique(),
            TaskId = taskId,
            Description = description,
            IsCompleted = false,
            Order = order,
            CreatedDate = DateTime.UtcNow
        };

        return item;
    }

    /// <summary>
    /// Updates the description
    /// </summary>
    public void UpdateDescription(string description)
    {
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (description.Length < 2)
            throw new ArgumentException("Checklist item description must be at least 2 characters", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Checklist item description cannot exceed 500 characters", nameof(description));

        Description = description;
    }

    /// <summary>
    /// Updates the order
    /// </summary>
    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        Order = order;
    }

    /// <summary>
    /// Marks item as completed
    /// </summary>
    public void Complete(Guid userId)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Checklist item is already completed");

        IsCompleted = true;
        CompletedByUserId = userId;
        CompletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks item as incomplete (unchecks)
    /// </summary>
    public void Uncomplete()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Checklist item is not completed");

        IsCompleted = false;
        CompletedByUserId = null;
        CompletedDate = null;
    }

    /// <summary>
    /// Toggles completion status
    /// </summary>
    public void Toggle(Guid userId)
    {
        if (IsCompleted)
        {
            Uncomplete();
        }
        else
        {
            Complete(userId);
        }
    }

    public override string ToString()
    {
        return $"ChecklistItem: {Description} ({(IsCompleted ? "✓" : "○")})";
    }
}
