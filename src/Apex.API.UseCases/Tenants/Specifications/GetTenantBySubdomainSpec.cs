using Ardalis.Specification;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.UseCases.Tenants.Specifications;

/// <summary>
/// Specification to find a tenant by subdomain
/// </summary>
public class GetTenantBySubdomainSpec : Specification<Tenant>
{
    public GetTenantBySubdomainSpec(string subdomain)
    {
        Query.Where(t => t.Subdomain == subdomain);
    }
}
