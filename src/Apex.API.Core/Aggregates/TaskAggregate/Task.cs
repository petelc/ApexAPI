using Traxs.SharedKernel;
using Ardalis.GuardClauses;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TaskAggregate.Events;

namespace Apex.API.Core.Aggregates.TaskAggregate;

/// <summary>
/// Task aggregate root - represents a unit of work within a project
/// Can be assigned to an individual user or to a department
/// ENHANCED: Added implementation notes, resolution notes, and user tracking
/// </summary>
public class Task : EntityBase, IAggregateRoot
{
    // Strong-typed ID
    private TaskId _id;
    public new TaskId Id
    {
        get => _id;
        private set => _id = value;
    }

    // Multi-tenant isolation
    private TenantId _tenantId;
    public TenantId TenantId
    {
        get => _tenantId;
        private set => _tenantId = value;
    }

    // Parent Project
    private ProjectId _projectId;
    public ProjectId ProjectId
    {
        get => _projectId;
        private set => _projectId = value;
    }

    // Core Information
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // ✅ NEW: Notes for tracking implementation and resolution
    public string? ImplementationNotes { get; private set; }
    public string? ResolutionNotes { get; private set; }

    // Status & Priority
    public Apex.API.Core.ValueObjects.TaskStatus Status { get; private set; } = Apex.API.Core.ValueObjects.TaskStatus.NotStarted;
    public RequestPriority Priority { get; private set; } = RequestPriority.Medium;

    // Assignment - Two modes:
    // Mode 1: Assigned to specific user (AssignedToUserId is set, AssignedToDepartmentId is null)
    // Mode 2: Assigned to department (AssignedToDepartmentId is set, any dept member can claim)
    // After claim: Both are set (user + their department)
    public Guid? AssignedToUserId { get; private set; }
    private DepartmentId? _assignedToDepartmentId;
    public DepartmentId? AssignedToDepartmentId
    {
        get => _assignedToDepartmentId;
        private set => _assignedToDepartmentId = value;
    }
    public string? AssignedToDepartmentName { get; private set; }

    // User Tracking
    public Guid CreatedByUserId { get; private set; }

    // ✅ NEW: Track who started and completed the task
    public Guid? StartedByUserId { get; private set; }
    public Guid? CompletedByUserId { get; private set; }

    // Time Tracking
    public decimal? EstimatedHours { get; private set; }
    public decimal ActualHours { get; private set; }

    // Dates
    public DateTime CreatedDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? StartedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    // Blocking
    public string? BlockedReason { get; private set; }
    public DateTime? BlockedDate { get; private set; }

    // Add to Task.cs aggregate

    private readonly List<TaskChecklistItem> _checklistItems = new();
    public IReadOnlyCollection<TaskChecklistItem> ChecklistItems => _checklistItems.AsReadOnly();

    private readonly List<TaskActivityLog> _activityLogs = new();
    public IReadOnlyCollection<TaskActivityLog> ActivityLogs => _activityLogs.AsReadOnly();

    /// <summary>
    /// Adds a checklist item to the task
    /// </summary>
    public TaskChecklistItem AddChecklistItem(string description, int order)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot add checklist items to task in {Status.Name} status");

        var item = TaskChecklistItem.Create(Id, description, order);
        _checklistItems.Add(item);
        LastModifiedDate = DateTime.UtcNow;

        return item;
    }

    /// <summary>
    /// Gets a specific checklist item
    /// </summary>
    public TaskChecklistItem? GetChecklistItem(TaskChecklistItemId itemId)
    {
        return _checklistItems.FirstOrDefault(i => i.Id == itemId);
    }

    /// <summary>
    /// Logs an activity
    /// </summary>
    public void LogActivity(TaskActivityType type, string description, Guid userId, string? details = null)
    {
        var activity = TaskActivityLog.Create(Id, type, description, userId, details);
        _activityLogs.Add(activity);
    }


    // EF Core constructor
    private Task() { }

    /// <summary>
    /// Creates a new task for a project (factory method)
    /// </summary>
    public static Task Create(
        TenantId tenantId,
        ProjectId projectId,
        string title,
        string description,
        RequestPriority priority,
        Guid createdByUserId,
        decimal? estimatedHours = null,
        DateTime? dueDate = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (title.Length < 3)
            throw new ArgumentException("Task title must be at least 3 characters", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Task title cannot exceed 200 characters", nameof(title));

        if (estimatedHours.HasValue && estimatedHours.Value <= 0)
            throw new ArgumentException("Estimated hours must be greater than 0", nameof(estimatedHours));

        var task = new Task
        {
            Id = TaskId.CreateUnique(),
            TenantId = tenantId,
            ProjectId = projectId,
            Title = title,
            Description = description,
            Priority = priority,
            Status = Apex.API.Core.ValueObjects.TaskStatus.NotStarted,
            CreatedByUserId = createdByUserId,
            CreatedDate = DateTime.UtcNow,
            EstimatedHours = estimatedHours,
            DueDate = dueDate,
            ActualHours = 0
        };

        task.RegisterDomainEvent(new TaskCreatedEvent(task.Id, projectId, title, createdByUserId));

        return task;
    }

    /// <summary>
    /// Updates task details
    /// </summary>
    public void Update(
        string title,
        string description,
        RequestPriority priority,
        decimal? estimatedHours = null,
        DateTime? dueDate = null)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot update task in {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        if (estimatedHours.HasValue && estimatedHours.Value <= 0)
            throw new ArgumentException("Estimated hours must be greater than 0", nameof(estimatedHours));

        Title = title;
        Description = description;
        Priority = priority;
        EstimatedHours = estimatedHours;
        DueDate = dueDate;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// ✅ NEW: Updates implementation notes
    /// </summary>
    public void UpdateImplementationNotes(string? notes)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot update implementation notes for task in {Status.Name} status");

        if (notes != null && notes.Length > 5000)
            throw new ArgumentException("Implementation notes cannot exceed 5000 characters", nameof(notes));

        ImplementationNotes = notes;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// ✅ NEW: Updates resolution notes (typically when completing)
    /// </summary>
    public void UpdateResolutionNotes(string? notes)
    {
        if (notes != null && notes.Length > 5000)
            throw new ArgumentException("Resolution notes cannot exceed 5000 characters", nameof(notes));

        ResolutionNotes = notes;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns task to a specific user
    /// </summary>
    public void AssignToUser(Guid userId, DepartmentId? userDepartmentId, string? departmentName, Guid assignedByUserId)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot assign task in {Status.Name} status");

        AssignedToUserId = userId;
        AssignedToDepartmentId = userDepartmentId;
        AssignedToDepartmentName = departmentName;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskAssignedToUserEvent(Id, userId, assignedByUserId));
    }

    /// <summary>
    /// Assigns task to a department (any member can claim it)
    /// </summary>
    public void AssignToDepartment(DepartmentId departmentId, string? departmentName, Guid assignedByUserId)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot assign task in {Status.Name} status");

        AssignedToUserId = null; // Clear individual assignment
        AssignedToDepartmentId = departmentId;
        AssignedToDepartmentName = departmentName;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskAssignedToDepartmentEvent(Id, departmentId, assignedByUserId));
    }

    /// <summary>
    /// User claims a task (simple version - no department required)
    /// </summary>
    public void ClaimTask(Guid userId)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot claim task in {Status.Name} status");

        if (AssignedToUserId.HasValue && AssignedToUserId.Value != userId)
            throw new InvalidOperationException("Task is already assigned to another user");

        AssignedToUserId = userId;
        AssignedToDepartmentName = null;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskAssignedToUserEvent(Id, userId, userId));
    }

    /// <summary>
    /// User claims a department-assigned task (original version)
    /// </summary>
    public void ClaimTask(Guid userId, DepartmentId userDepartmentId)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException($"Cannot claim task in {Status.Name} status");

        if (!AssignedToDepartmentId.HasValue)
            throw new InvalidOperationException("Task is not assigned to a department");

        if (AssignedToDepartmentId != userDepartmentId)
            throw new InvalidOperationException("User is not in the department this task is assigned to");

        if (AssignedToUserId.HasValue)
            throw new InvalidOperationException("Task has already been claimed");

        AssignedToUserId = userId;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskClaimedEvent(Id, userId, userDepartmentId));
    }


    /// <summary>
    /// Starts the task (NotStarted → InProgress)
    /// Auto-assigns to user if not already assigned
    /// </summary>
    public void Start(Guid startedByUserId)
    {
        if (!Status.CanStart)
            throw new InvalidOperationException($"Cannot start task from {Status.Name} status");

        // ✅ AUTO-ASSIGN: If not assigned, assign to the user starting it
        if (!AssignedToUserId.HasValue)
        {
            AssignedToUserId = startedByUserId;
        }

        Status = Apex.API.Core.ValueObjects.TaskStatus.InProgress;
        StartedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskStartedEvent(Id, startedByUserId));
    }

    /// <summary>
    /// Logs time worked on the task
    /// </summary>
    public void LogTime(decimal hours)
    {
        if (!Status.CanLogTime)
            throw new InvalidOperationException($"Cannot log time for task in {Status.Name} status");

        if (hours <= 0)
            throw new ArgumentException("Hours must be greater than 0", nameof(hours));

        ActualHours += hours;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskTimeLoggedEvent(Id, hours, ActualHours));
    }

    /// <summary>
    /// Blocks the task (InProgress → Blocked)
    /// </summary>
    public void Block(string reason, Guid blockedByUserId)
    {
        if (!Status.CanBlock)
            throw new InvalidOperationException($"Cannot block task from {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        Status = Apex.API.Core.ValueObjects.TaskStatus.Blocked;
        BlockedReason = reason;
        BlockedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskBlockedEvent(Id, reason, blockedByUserId));
    }

    /// <summary>
    /// Unblocks the task (Blocked → InProgress)
    /// </summary>
    public void Unblock(Guid unblockedByUserId)
    {
        if (!Status.CanUnblock)
            throw new InvalidOperationException($"Cannot unblock task from {Status.Name} status");

        Status = Apex.API.Core.ValueObjects.TaskStatus.InProgress;
        BlockedReason = null;
        BlockedDate = null;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskUnblockedEvent(Id, unblockedByUserId));
    }

    /// <summary>
    /// Completes the task (InProgress → Completed)
    /// ✅ ENHANCED: Now tracks who completed the task
    /// </summary>
    public void Complete(Guid completedByUserId, string? resolutionNotes = null)
    {
        if (!Status.CanComplete)
            throw new InvalidOperationException($"Cannot complete task from {Status.Name} status");

        Status = Apex.API.Core.ValueObjects.TaskStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        CompletedByUserId = completedByUserId;  // ✅ Track who completed

        if (resolutionNotes != null)
        {
            UpdateResolutionNotes(resolutionNotes);
        }

        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskCompletedEvent(Id, completedByUserId));
    }

    /// <summary>
    /// Cancels the task
    /// </summary>
    public void Cancel(Guid cancelledByUserId, string reason)
    {
        if (!Status.CanCancel)
            throw new InvalidOperationException($"Cannot cancel task from {Status.Name} status");

        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        Status = Apex.API.Core.ValueObjects.TaskStatus.Cancelled;
        LastModifiedDate = DateTime.UtcNow;

        RegisterDomainEvent(new TaskCancelledEvent(Id, cancelledByUserId, reason));
    }

    /// <summary>
    /// Checks if task is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return DueDate.HasValue &&
               DateTime.UtcNow > DueDate.Value &&
               !Status.IsTerminal;
    }

    /// <summary>
    /// Gets days until due (negative if overdue)
    /// </summary>
    public int? GetDaysUntilDue()
    {
        if (!DueDate.HasValue) return null;
        return (DueDate.Value - DateTime.UtcNow).Days;
    }

    /// <summary>
    /// Gets variance between estimated and actual hours
    /// </summary>
    public decimal? GetHoursVariance()
    {
        if (!EstimatedHours.HasValue) return null;
        return ActualHours - EstimatedHours.Value;
    }

    /// <summary>
    /// Gets percentage of estimated hours used
    /// </summary>
    public decimal? GetEstimatedHoursPercentage()
    {
        if (!EstimatedHours.HasValue || EstimatedHours.Value == 0) return null;
        return (ActualHours / EstimatedHours.Value) * 100;
    }

    public override string ToString()
    {
        return $"Task: {Title} ({Status.Name})";
    }
}
