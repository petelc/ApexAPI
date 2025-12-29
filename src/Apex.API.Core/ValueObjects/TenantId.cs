using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for Tenant aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct TenantId
{
    /// <summary>
    /// Creates a new unique TenantId
    /// </summary>
    public static TenantId CreateUnique() => From(Guid.NewGuid());

    /// <summary>
    /// Empty/default TenantId (for comparison)
    /// </summary>
    public static TenantId Empty => From(Guid.Empty);
}
