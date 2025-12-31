using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for ProjectRequest aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct ProjectRequestId
{
    /// <summary>
    /// Creates a new unique ProjectRequestId
    /// </summary>
    public static ProjectRequestId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default ProjectRequestId (for comparison)
    /// </summary>
    public static ProjectRequestId Empty => From(Guid.Empty);
}
