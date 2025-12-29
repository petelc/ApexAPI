using Ardalis.Specification;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.Core.Aggregates.TenantAggregate.Specifications;

/// <summary>
/// Specification to find a tenant by subdomain
/// </summary>
public sealed class TenantBySubdomainSpec : Specification<Tenant>, ISingleResultSpecification<Tenant>
{
    public TenantBySubdomainSpec(string subdomain)
    {
        Query
            .Where(t => t.Subdomain == subdomain.ToLower())
            .AsNoTracking(); // Read-only operation
    }
}
