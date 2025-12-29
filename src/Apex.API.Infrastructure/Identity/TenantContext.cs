using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Infrastructure.Data;

namespace Apex.API.Infrastructure.Identity;

/// <summary>
/// ULTRA-SIMPLIFIED: Uses ApexDbContext directly from DI
/// This works because TenantContext is scoped and only created during requests
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantContext> _logger;
    private readonly ApexDbContext _dbContext;

    private const int CacheDurationMinutes = 10;

    public DeploymentMode DeploymentMode { get; }
    public bool IsMultiTenant => DeploymentMode == DeploymentMode.SaaS;
    public string CurrentTenantSchema { get; private set; } = "dbo";
    public TenantId CurrentTenantId { get; private set; } = TenantId.Empty;

    public TenantContext(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<TenantContext> logger,
        ApexDbContext dbContext)  // Just inject DbContext directly - it's scoped!
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
        _dbContext = dbContext;

        var mode = _configuration["Deployment:Mode"] ?? "SaaS";
        DeploymentMode = DeploymentMode.FromName(mode);

        _logger.LogInformation("TenantContext initialized in {Mode} mode", DeploymentMode.Name);
    }

    public async Task<Tenant> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        if (IsMultiTenant)
        {
            return await GetTenantFromSubdomainAsync(cancellationToken);
        }
        else
        {
            return await GetTenantFromConfigurationAsync(cancellationToken);
        }
    }

    private async Task<Tenant> GetTenantFromSubdomainAsync(CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is not available");

        var host = httpContext.Request.Host.Host;
        var subdomain = ExtractSubdomain(host);

        if (string.IsNullOrEmpty(subdomain))
        {
            throw new InvalidOperationException($"Could not extract subdomain from host: {host}");
        }

        var cacheKey = $"tenant:subdomain:{subdomain}";
        if (_cache.TryGetValue<Tenant>(cacheKey, out var cachedTenant) && cachedTenant != null)
        {
            CurrentTenantSchema = cachedTenant.SchemaName;
            CurrentTenantId = cachedTenant.Id;
            return cachedTenant;
        }

        // Use the injected DbContext
        var tenant = await _dbContext.Tenants
            .Where(t => t.Subdomain == subdomain)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with subdomain '{subdomain}' not found");
        }

        _cache.Set(cacheKey, tenant, TimeSpan.FromMinutes(CacheDurationMinutes));

        CurrentTenantSchema = tenant.SchemaName;
        CurrentTenantId = tenant.Id;

        _logger.LogDebug("Resolved tenant {TenantId} from subdomain {Subdomain}", 
            tenant.Id, subdomain);

        return tenant;
    }

    private async Task<Tenant> GetTenantFromConfigurationAsync(CancellationToken cancellationToken)
    {
        var tenantIdString = _configuration["Deployment:TenantId"]
            ?? throw new InvalidOperationException("TenantId not configured for self-hosted mode");

        if (!Guid.TryParse(tenantIdString, out var tenantGuid))
        {
            throw new InvalidOperationException($"Invalid TenantId in configuration: {tenantIdString}");
        }

        var tenantId = TenantId.From(tenantGuid);

        var cacheKey = $"tenant:id:{tenantId.Value}";
        if (_cache.TryGetValue<Tenant>(cacheKey, out var cachedTenant) && cachedTenant != null)
        {
            CurrentTenantSchema = "dbo";
            CurrentTenantId = cachedTenant.Id;
            return cachedTenant;
        }

        // Use the injected DbContext
        var tenant = await _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{tenantId}' not found");
        }

        _cache.Set(cacheKey, tenant, TimeSpan.FromMinutes(CacheDurationMinutes));

        CurrentTenantSchema = "dbo";
        CurrentTenantId = tenant.Id;

        _logger.LogDebug("Resolved tenant {TenantId} from configuration", tenant.Id);

        return tenant;
    }

    private string ExtractSubdomain(string host)
    {
        var baseDomain = _configuration["Deployment:BaseDomain"] ?? "localhost";

        if (host.Contains(':'))
        {
            host = host.Split(':')[0];
        }

        if (host.Equals(baseDomain, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"No subdomain found in host: {host}");
        }

        var subdomain = host.Replace($".{baseDomain}", "", StringComparison.OrdinalIgnoreCase);

        if (!System.Text.RegularExpressions.Regex.IsMatch(subdomain, @"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$"))
        {
            throw new InvalidOperationException($"Invalid subdomain format: {subdomain}");
        }

        return subdomain;
    }
}
