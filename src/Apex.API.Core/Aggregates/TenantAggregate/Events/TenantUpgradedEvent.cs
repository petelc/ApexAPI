using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.TenantAggregate.Events;

/// <summary>
/// Raised when a tenant upgrades their subscription tier
/// </summary>
public sealed class TenantUpgradedEvent : DomainEventBase
{
    public TenantId TenantId { get; }
    public SubscriptionTier OldTier { get; }
    public SubscriptionTier NewTier { get; }

    public TenantUpgradedEvent(
        TenantId tenantId, 
        SubscriptionTier oldTier, 
        SubscriptionTier newTier)
    {
        TenantId = tenantId;
        OldTier = oldTier;
        NewTier = newTier;
    }
}
