using Ardalis.SmartEnum;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Deployment mode for APEX installation
/// </summary>
public sealed class DeploymentMode : SmartEnum<DeploymentMode>
{
    /// <summary>
    /// Multi-tenant SaaS deployment (primary)
    /// </summary>
    public static readonly DeploymentMode SaaS = new(nameof(SaaS), 0, isMultiTenant: true);

    /// <summary>
    /// Single-tenant self-hosted deployment
    /// </summary>
    public static readonly DeploymentMode SelfHosted = new(nameof(SelfHosted), 1, isMultiTenant: false);

    /// <summary>
    /// Whether this mode supports multiple tenants
    /// </summary>
    public bool IsMultiTenant { get; }

    /// <summary>
    /// Whether this mode requires tenant resolution
    /// </summary>
    public bool RequiresTenantResolution => IsMultiTenant;

    private DeploymentMode(string name, int value, bool isMultiTenant) : base(name, value)
    {
        IsMultiTenant = isMultiTenant;
    }
}
