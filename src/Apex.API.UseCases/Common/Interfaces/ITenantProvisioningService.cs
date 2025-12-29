using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Common.Interfaces;

/// <summary>
/// Service for provisioning new tenants (schema creation, seeding, etc.)
/// </summary>
public interface ITenantProvisioningService
{
    /// <summary>
    /// Provisions a new tenant's infrastructure (creates schema, runs migrations, seeds data)
    /// </summary>
    Task ProvisionTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deprovisions a tenant (removes schema and all data)
    /// </summary>
    Task DeprovisionTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a tenant's schema exists
    /// </summary>
    Task<bool> SchemaExistsAsync(string schemaName, CancellationToken cancellationToken = default);
}
