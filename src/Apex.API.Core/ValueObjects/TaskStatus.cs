using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Task lifecycle status
/// </summary>
public sealed class TaskStatus : SmartEnum<TaskStatus>
{
    /// <summary>
    /// NotStarted - Task created but not yet started
    /// </summary>
    public static readonly TaskStatus NotStarted = new(nameof(NotStarted), 0);

    /// <summary>
    /// InProgress - Task is being worked on
    /// </summary>
    public static readonly TaskStatus InProgress = new(nameof(InProgress), 1);

    /// <summary>
    /// Blocked - Task is blocked by external dependency or issue
    /// </summary>
    public static readonly TaskStatus Blocked = new(nameof(Blocked), 2);

    /// <summary>
    /// Completed - Task successfully finished
    /// </summary>
    public static readonly TaskStatus Completed = new(nameof(Completed), 3);

    /// <summary>
    /// Cancelled - Task was cancelled
    /// </summary>
    public static readonly TaskStatus Cancelled = new(nameof(Cancelled), 4);

    private TaskStatus(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Whether the task can be started
    /// </summary>
    public bool CanStart => this == NotStarted;

    /// <summary>
    /// Whether the task can be blocked
    /// </summary>
    public bool CanBlock => this == InProgress;

    /// <summary>
    /// Whether the task can be unblocked
    /// </summary>
    public bool CanUnblock => this == Blocked;

    /// <summary>
    /// Whether the task can be completed
    /// </summary>
    public bool CanComplete => this == InProgress;

    /// <summary>
    /// Whether the task can be cancelled
    /// </summary>
    public bool CanCancel => this == NotStarted || this == InProgress || this == Blocked;

    /// <summary>
    /// Whether the task is in a terminal state
    /// </summary>
    public bool IsTerminal => this == Completed || this == Cancelled;

    /// <summary>
    /// Whether the task is active (accepting work)
    /// </summary>
    public bool IsActive => this == InProgress;

    /// <summary>
    /// Whether time can be logged on this task
    /// </summary>
    public bool CanLogTime => this == InProgress || this == Blocked;
}
