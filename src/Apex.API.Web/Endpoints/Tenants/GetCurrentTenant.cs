using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.Tenants;

public record GetCurrentTenantResponse
{
    public Guid TenantId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string SchemaName { get; init; } = string.Empty;
    public string SubscriptionTier { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string DeploymentMode { get; init; } = string.Empty;
    public bool IsMultiTenant { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class GetCurrentTenantEndpoint : EndpointWithoutRequest<GetCurrentTenantResponse>
{
    private readonly ITenantContext _tenantContext;

    public GetCurrentTenantEndpoint(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override void Configure()
    {
        Get("/tenants/current");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var tenant = await _tenantContext.GetCurrentTenantAsync(ct);

            var response = new GetCurrentTenantResponse
            {
                TenantId = tenant.Id.Value,
                CompanyName = tenant.CompanyName,
                Subdomain = tenant.Subdomain,
                SchemaName = tenant.SchemaName,
                SubscriptionTier = tenant.Tier.Name,
                Status = tenant.Status.Name,
                IsActive = tenant.IsActive,
                DeploymentMode = _tenantContext.DeploymentMode.Name,
                IsMultiTenant = _tenantContext.IsMultiTenant,
                Message = _tenantContext.IsMultiTenant
                    ? $"Tenant resolved from subdomain: {tenant.Subdomain}"
                    : $"Tenant resolved from configuration (self-hosted mode)"
            };

            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { Error = $"Failed to resolve tenant: {ex.Message}" }, ct);
        }
    }
}