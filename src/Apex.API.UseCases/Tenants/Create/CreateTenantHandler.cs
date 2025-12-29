using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Traxs.SharedKernel;
using Apex.API.Core.ValueObjects;
using Apex.API.Core.Aggregates.TenantAggregate;
using Apex.API.Core.Aggregates.TenantAggregate.Specifications;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tenants.Create;

/// <summary>
/// Handles tenant creation (company signup)
/// </summary>
public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Result<TenantId>>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IReadRepository<Tenant> _tenantReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvisioningService _provisioningService;
    private readonly ILogger<CreateTenantHandler> _logger;

    public CreateTenantHandler(
        IRepository<Tenant> tenantRepository,
        IReadRepository<Tenant> tenantReadRepository,
        IUnitOfWork unitOfWork,
        ITenantProvisioningService provisioningService,
        ILogger<CreateTenantHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _tenantReadRepository = tenantReadRepository;
        _unitOfWork = unitOfWork;
        _provisioningService = provisioningService;
        _logger = logger;
    }

    public async ValueTask<Result<TenantId>> Handle(
    CreateTenantCommand request,
    CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating tenant: Company={CompanyName}, Subdomain={Subdomain}",
            request.CompanyName,
            request.Subdomain);

        try
        {
            // 1. Check if subdomain is already taken
            var subdomainSpec = new TenantBySubdomainSpec(request.Subdomain);
            var existingTenant = await _tenantReadRepository.FirstOrDefaultAsync(
                subdomainSpec,
                cancellationToken);

            if (existingTenant != null)
            {
                _logger.LogWarning(
                    "Subdomain already taken: {Subdomain}",
                    request.Subdomain);

                return Result<TenantId>.Error(
                    $"Subdomain '{request.Subdomain}' is already taken. Please choose another.");
            }

            // 2. Create tenant entity (starts in Trial tier)
            var tenant = Tenant.Create(
                request.CompanyName,
                request.Subdomain,
                request.Region);

            // 3. Save tenant to database (in shared schema)
            await _tenantRepository.AddAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Tenant created in database: TenantId={TenantId}, Subdomain={Subdomain}",
                tenant.Id,
                tenant.Subdomain);

            // 4. Provision tenant infrastructure (create schema, run migrations, seed data)
            try
            {
                await _provisioningService.ProvisionTenantAsync(tenant.Id, cancellationToken);

                _logger.LogInformation(
                    "Tenant provisioned successfully: TenantId={TenantId}, Schema={Schema}",
                    tenant.Id,
                    tenant.SchemaName);
            }
            catch (Exception provisionEx)
            {
                _logger.LogError(
                    provisionEx,
                    "Failed to provision tenant: TenantId={TenantId}",
                    tenant.Id);

                // Provisioning failed - clean up the tenant record
                await _tenantRepository.DeleteAsync(tenant, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<TenantId>.Error(
                    "Failed to provision tenant infrastructure. Please try again or contact support.");
            }

            // 5. TODO: Create admin user account
            // This will be implemented when we build the User aggregate
            // await _userService.CreateAdminUserAsync(
            //     tenant.Id, 
            //     request.AdminEmail, 
            //     request.AdminFirstName, 
            //     request.AdminLastName,
            //     cancellationToken);

            // 6. TODO: Send welcome email
            // await _emailService.SendWelcomeEmailAsync(
            //     request.AdminEmail,
            //     request.CompanyName,
            //     tenant.Subdomain,
            //     cancellationToken);

            _logger.LogInformation(
                "Tenant signup complete: TenantId={TenantId}, Company={CompanyName}, Subdomain={Subdomain}",
                tenant.Id,
                tenant.CompanyName,
                tenant.Subdomain);

            // Domain events will be dispatched automatically after SaveChanges
            // (TenantCreatedEvent -> can trigger welcome email, analytics, etc.)

            return Result<TenantId>.Success(tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error creating tenant: Company={CompanyName}, Subdomain={Subdomain}",
                request.CompanyName,
                request.Subdomain);

            return Result<TenantId>.Error(
                "An unexpected error occurred during signup. Please try again.");
        }
    }
}
