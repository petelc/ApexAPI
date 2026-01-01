using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Project aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct ProjectId
{
    /// <summary>
    /// Creates a new unique ProjectId
    /// </summary>
    public static ProjectId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default ProjectId (for comparison)
    /// </summary>
    public static ProjectId Empty => From(Guid.Empty);
}
