using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;

namespace Apex.API.Core.Aggregates.TenantAggregate.Events;

/// <summary>
/// Raised when a tenant is suspended
/// </summary>
public sealed class TenantSuspendedEvent : DomainEventBase
{
    public TenantId TenantId { get; }
    public string Reason { get; }

    public TenantSuspendedEvent(TenantId tenantId, string reason)
    {
        TenantId = tenantId;
        Reason = reason;
    }
}
