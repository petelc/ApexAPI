using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;
using Apex.API.UseCases.Tenants.Specifications;

namespace Apex.API.UseCases.Tenants.Create;

/// <summary>
/// Handles the CreateTenantCommand (using MediatR)
/// </summary>
public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Result<TenantId>>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly ITenantProvisioningService _provisioningService;
    private readonly ILogger<CreateTenantHandler> _logger;

    public CreateTenantHandler(
        IRepository<Tenant> tenantRepository,
        ITenantProvisioningService provisioningService,
        ILogger<CreateTenantHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _provisioningService = provisioningService;
        _logger = logger;
    }

    public async Task<Result<TenantId>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating tenant: Company={CompanyName}, Subdomain={Subdomain}",
                request.CompanyName,
                request.Subdomain);

            // Check if subdomain already exists
            var existingTenant = await _tenantRepository
                .FirstOrDefaultAsync(new GetTenantBySubdomainSpec(request.Subdomain), cancellationToken);

            if (existingTenant != null)
            {
                _logger.LogWarning(
                    "Subdomain already exists: {Subdomain}",
                    request.Subdomain);

                return Result<TenantId>.Error("Subdomain already exists. Please choose a different subdomain.");
            }

            var tenant = string.IsNullOrWhiteSpace(request.Region)
                ? Tenant.Create(request.CompanyName, request.Subdomain)
                : Tenant.Create(request.CompanyName, request.Subdomain, request.Region);

            // ✅ ADD THIS DIAGNOSTIC CODE:
            _logger.LogInformation(
                "DIAGNOSTIC: Tenant created in memory - " +
                "Subdomain={Subdomain}, SchemaName={SchemaName}, SchemaNameIsNull={IsNull}",
                tenant.Subdomain,
                tenant.SchemaName,
                tenant.SchemaName == null);

            // Continue with save...
            await _tenantRepository.AddAsync(tenant, cancellationToken);

            // Save to database (this will also dispatch domain events)
            //await _tenantRepository.AddAsync(tenant, cancellationToken);

            // Provision tenant schema
            await _provisioningService.ProvisionTenantAsync(tenant.Id, cancellationToken);

            _logger.LogInformation(
                "Tenant created successfully: TenantId={TenantId}, Subdomain={Subdomain}",
                tenant.Id.Value,
                tenant.Subdomain);

            return Result<TenantId>.Success(tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error creating tenant: Company={CompanyName}, Subdomain={Subdomain}",
                request.CompanyName,
                request.Subdomain);

            return Result<TenantId>.Error("An unexpected error occurred while creating the tenant.");
        }
    }
}
