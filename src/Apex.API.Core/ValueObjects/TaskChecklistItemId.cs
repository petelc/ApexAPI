using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed ID for TaskChecklistItem
/// </summary>
[ValueObject<Guid>]
public readonly partial struct TaskChecklistItemId
{
    public static TaskChecklistItemId CreateUnique() => From(Guid.NewGuid());
}
