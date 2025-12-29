using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.UseCases.Tenants.Create;

namespace Apex.API.Web.Endpoints.Tenants;

/// <summary>
/// Endpoint for tenant signup (bypasses Mediator for now)
/// </summary>
public class CreateTenantEndpoint : Endpoint<CreateTenantRequest, CreateTenantResponse>
{
    private readonly CreateTenantHandler _handler;
    private readonly IReadRepository<Tenant> _tenantRepository;
    private readonly IConfiguration _configuration;

    public CreateTenantEndpoint(
        CreateTenantHandler handler,  // Inject handler directly
        IReadRepository<Tenant> tenantRepository,
        IConfiguration configuration)
    {
        _handler = handler;
        _tenantRepository = tenantRepository;
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/api/tenants/signup");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateTenantRequest req, CancellationToken ct)
    {
        // Create command
        var command = new CreateTenantCommand(
            req.CompanyName,
            req.Subdomain,
            req.AdminEmail,
            req.AdminFirstName,
            req.AdminLastName,
            req.Region);

        // Call handler directly (bypass Mediator)
        var result = await _handler.Handle(command, ct);

        if (result.IsSuccess)
        {
            var tenant = await _tenantRepository.GetByIdAsync(result.Value.Value, ct);

            if (tenant == null)
            {
                var fallbackResponse = new CreateTenantResponse
                {
                    TenantId = result.Value.Value,
                    CompanyName = req.CompanyName,
                    Subdomain = req.Subdomain,
                    TenantUrl = GetTenantUrl(req.Subdomain),
                    SubscriptionTier = "Trial",
                    Message = "Your account has been created successfully!"
                };

                await HttpContext.Response.WriteAsJsonAsync(fallbackResponse, ct);
                HttpContext.Response.StatusCode = StatusCodes.Status201Created;
                return;
            }

            var response = new CreateTenantResponse
            {
                TenantId = tenant.Id.Value,
                CompanyName = tenant.CompanyName,
                Subdomain = tenant.Subdomain,
                TenantUrl = GetTenantUrl(tenant.Subdomain),
                SubscriptionTier = tenant.Tier.Name,
                TrialEndsDate = tenant.TrialEndsDate,
                TrialDaysRemaining = tenant.GetTrialDaysRemaining(),
                Message = $"Welcome to APEX, {req.CompanyName}! Your account has been created successfully. " +
                         $"You have {tenant.GetTrialDaysRemaining()} days remaining in your free trial."
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            HttpContext.Response.Headers.Location = $"/api/tenants/{tenant.Id.Value}";
            await HttpContext.Response.WriteAsJsonAsync(response, ct);
        }
        else
        {
            var errors = result.Errors.Select(e => new { Error = e }).ToList();
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { Errors = errors }, ct);
        }
    }

    private string GetTenantUrl(string subdomain)
    {
        var baseDomain = _configuration["Deployment:BaseDomain"] ?? "apex.cloud";
        var protocol = _configuration["Deployment:Protocol"] ?? "https";
        return $"{protocol}://{subdomain}.{baseDomain}";
    }
}