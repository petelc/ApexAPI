using MediatR;
using Traxs.SharedKernel;

namespace Apex.API.Core.Aggregates.TenantAggregate.Events;

/// <summary>
/// Domain event raised when a new tenant is created (using MediatR)
/// Inherits from DomainEventBase (for domain event infrastructure)
/// Implements INotification (for MediatR dispatching)
/// </summary>
public class TenantCreatedEvent : DomainEventBase, INotification
{
    public Guid TenantId { get; }
    public string CompanyName { get; }
    public string Subdomain { get; }
    public string SchemaName { get; }

    public TenantCreatedEvent(
        Guid tenantId,
        string companyName,
        string subdomain,
        string schemaName)
    {
        TenantId = tenantId;
        CompanyName = companyName;
        Subdomain = subdomain;
        SchemaName = schemaName;
    }
}
