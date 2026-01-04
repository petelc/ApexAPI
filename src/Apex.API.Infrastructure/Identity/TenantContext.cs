using System.Security.Claims;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TenantAggregate;
using Microsoft.AspNetCore.Http;

namespace Apex.API.Infrastructure.Identity;

/// <summary>
/// Provides tenant context from the current HTTP request's authenticated user
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public TenantContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public TenantId CurrentTenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                // Not authenticated - return empty TenantId
                return TenantId.Empty;
            }

            // ✅ Try multiple claim types for tenantId (case variations)
            var tenantIdClaim = httpContext.User.FindFirst("TenantId")    // ← Your JwtTokenService uses this
                ?? httpContext.User.FindFirst("tenantId")
                ?? httpContext.User.FindFirst(ClaimTypes.GroupSid)
                ?? httpContext.User.FindFirst("tenant_id");

            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return TenantId.From(tenantId);
            }

            // ✅ LOG WARNING: TenantId not found in claims
            Console.WriteLine("WARNING: TenantId claim not found in JWT token!");
            Console.WriteLine($"Available claims: {string.Join(", ", httpContext.User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

            return TenantId.Empty;
        }
    }

    public string CurrentTenantSchema
    {
        get
        {
            var tenantId = CurrentTenantId;
            if (tenantId == TenantId.Empty)
                return "shared"; // Default to shared schema

            // Schema naming: tenant_<guid>
            return $"tenant_{tenantId.Value.ToString().Replace("-", "").ToLowerInvariant()}";
        }
    }

    public Guid CurrentUserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return Guid.Empty;
            }

            // Try standard claim types
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirst("sub")
                ?? httpContext.User.FindFirst("userId");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return Guid.Empty;
        }
    }

    public DeploymentMode DeploymentMode
    {
        get
        {
            var mode = _configuration["Deployment:Mode"];
            return mode?.ToLowerInvariant() switch
            {
                "selfhosted" => DeploymentMode.SelfHosted,
                "saas" => DeploymentMode.SaaS,
                _ => DeploymentMode.SaaS // Default to SaaS
            };
        }
    }

    public bool IsMultiTenant
    {
        get
        {
            var multiTenant = _configuration["Deployment:MultiTenant"];
            if (bool.TryParse(multiTenant, out var result))
                return result;

            // Default: SaaS mode = multi-tenant
            return DeploymentMode == DeploymentMode.SaaS;
        }
    }

    public async Task<Tenant> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        // This would typically query the tenant repository
        // For now, throw NotImplementedException as this is optional
        // You can implement this later when you have the Tenant repository injected
        throw new NotImplementedException("GetCurrentTenantAsync not yet implemented. Inject ITenantRepository to enable this feature.");
    }
}