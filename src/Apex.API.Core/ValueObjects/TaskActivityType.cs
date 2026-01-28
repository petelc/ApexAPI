using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Smart enum for task activity types (audit log)
/// </summary>
public sealed class TaskActivityType : SmartEnum<TaskActivityType>
{
    public static readonly TaskActivityType Created = new(nameof(Created), 1);
    public static readonly TaskActivityType Updated = new(nameof(Updated), 2);
    public static readonly TaskActivityType Assigned = new(nameof(Assigned), 3);
    public static readonly TaskActivityType Claimed = new(nameof(Claimed), 4);
    public static readonly TaskActivityType Started = new(nameof(Started), 5);
    public static readonly TaskActivityType Blocked = new(nameof(Blocked), 6);
    public static readonly TaskActivityType Unblocked = new(nameof(Unblocked), 7);
    public static readonly TaskActivityType Completed = new(nameof(Completed), 8);
    public static readonly TaskActivityType Cancelled = new(nameof(Cancelled), 9);
    public static readonly TaskActivityType TimeLogged = new(nameof(TimeLogged), 10);
    public static readonly TaskActivityType CommentAdded = new(nameof(CommentAdded), 11);
    public static readonly TaskActivityType ChecklistItemAdded = new(nameof(ChecklistItemAdded), 12);
    public static readonly TaskActivityType ChecklistItemCompleted = new(nameof(ChecklistItemCompleted), 13);
    public static readonly TaskActivityType NotesUpdated = new(nameof(NotesUpdated), 14);

    private TaskActivityType(string name, int value) : base(name, value) { }
}
