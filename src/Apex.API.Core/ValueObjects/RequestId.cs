using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Request aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct RequestId
{
    /// <summary>
    /// Creates a new unique RequestId
    /// </summary>
    public static RequestId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default RequestId (for comparison)
    /// </summary>
    public static RequestId Empty => From(Guid.Empty);
}
