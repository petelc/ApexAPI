using System;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.Web.Endpoints.Tenants;

public record GetTenantRequest
{
    public Guid Id { get; init; }
}

public record GetTenantResponse
{
    public Guid TenantId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string SchemaName { get; init; } = string.Empty;
    public string SubscriptionTier { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? TrialEndsDate { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public int MaxUsers { get; init; }
    public int MaxRequestsPerMonth { get; init; }
    public int MaxStorageGB { get; init; }
}

public class GetTenantEndpoint : Endpoint<GetTenantRequest, GetTenantResponse>
{
    private readonly IReadRepository<Tenant> _tenantRepository;

    public GetTenantEndpoint(IReadRepository<Tenant> tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public override void Configure()
    {
        Get("/api/tenants/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTenantRequest req, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(req.Id, ct);

        if (tenant == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var response = new GetTenantResponse
        {
            TenantId = tenant.Id.Value,
            CompanyName = tenant.CompanyName,
            Subdomain = tenant.Subdomain,
            SchemaName = tenant.SchemaName,
            SubscriptionTier = tenant.Tier.Name,
            Status = tenant.Status.Name,
            IsActive = tenant.IsActive,
            CreatedDate = tenant.CreatedDate,
            TrialEndsDate = tenant.TrialEndsDate,
            TrialDaysRemaining = tenant.GetTrialDaysRemaining(),
            MaxUsers = tenant.MaxUsers,
            MaxRequestsPerMonth = tenant.MaxRequestsPerMonth,
            MaxStorageGB = tenant.MaxStorageGB
        };

        await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
}