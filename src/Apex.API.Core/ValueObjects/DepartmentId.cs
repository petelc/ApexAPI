using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Department aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct DepartmentId
{
    /// <summary>
    /// Creates a new unique DepartmentId
    /// </summary>
    public static DepartmentId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default DepartmentId (for comparison)
    /// </summary>
    public static DepartmentId Empty => From(Guid.Empty);
}
