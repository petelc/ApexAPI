using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed ID for TaskActivityLog
/// </summary>
[ValueObject<Guid>]
public readonly partial struct TaskActivityLogId
{
    public static TaskActivityLogId CreateUnique() => From(Guid.NewGuid());
}
