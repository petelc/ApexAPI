using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.TenantAggregate.Events;

/// <summary>
/// Raised when a new tenant is created
/// </summary>
public sealed class TenantCreatedEvent : DomainEventBase
{
    public TenantId TenantId { get; }
    public string CompanyName { get; }
    public string Subdomain { get; }
    public SubscriptionTier Tier { get; }

    public TenantCreatedEvent(
        TenantId tenantId, 
        string companyName, 
        string subdomain,
        SubscriptionTier tier)
    {
        TenantId = tenantId;
        CompanyName = companyName;
        Subdomain = subdomain;
        Tier = tier;
    }
}
