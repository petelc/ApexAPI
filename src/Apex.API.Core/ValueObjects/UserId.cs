using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for User entity
/// </summary>
[ValueObject<Guid>]
public readonly partial struct UserId
{
    /// <summary>
    /// Creates a new unique UserId
    /// </summary>
    public static UserId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default UserId (for comparison)
    /// </summary>
    public static UserId Empty => From(Guid.Empty);
}
