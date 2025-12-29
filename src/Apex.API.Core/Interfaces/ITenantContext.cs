using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.Core.Interfaces;

/// <summary>
/// Provides context about the current tenant in multi-tenant scenarios
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant ID from the request context
    /// </summary>
    TenantId CurrentTenantId { get; }

    /// <summary>
    /// Gets the database schema name for the current tenant
    /// </summary>
    string CurrentTenantSchema { get; }

    /// <summary>
    /// Gets the current tenant entity (cached)
    /// </summary>
    Task<Tenant> GetCurrentTenantAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the deployment mode (SaaS vs Self-Hosted)
    /// </summary>
    DeploymentMode DeploymentMode { get; }

    /// <summary>
    /// Whether the system is running in multi-tenant mode
    /// </summary>
    bool IsMultiTenant { get; }
}
