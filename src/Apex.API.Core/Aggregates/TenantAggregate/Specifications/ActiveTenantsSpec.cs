using Ardalis.Specification;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.Core.Aggregates.TenantAggregate.Specifications;

/// <summary>
/// Specification to get all active tenants
/// </summary>
public sealed class ActiveTenantsSpec : Specification<Tenant>
{
    public ActiveTenantsSpec()
    {
        Query
            .Where(t => t.IsActive)
            .OrderBy(t => t.CompanyName);
    }
}
