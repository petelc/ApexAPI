using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Task aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct TaskId
{
    /// <summary>
    /// Creates a new unique TaskId
    /// </summary>
    public static TaskId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default TaskId (for comparison)
    /// </summary>
    public static TaskId Empty => From(Guid.Empty);
}
