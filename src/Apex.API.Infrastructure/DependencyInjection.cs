using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Infrastructure.Data;
using Apex.API.Infrastructure.Identity;
using Apex.API.Infrastructure.Services;
using Apex.API.UseCases.Common.Interfaces;
using Apex.API.UseCases.Tenants.Create;
using Apex.API.UseCases.Tenants.Events;
using Apex.API.Core.Aggregates.TenantAggregate.Events;
using Mediator;

namespace Apex.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // DbContext
        services.AddDbContext<ApexDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApexDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // HTTP Context
        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        // Tenant context
        services.AddScoped<ITenantContext, TenantContext>();

        // Repository pattern
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Domain event dispatcher
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        // Services
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

        // ========================================================================
        // HANDLERS - Direct registration (bypass Mediator for commands)
        // ========================================================================
        services.AddScoped<CreateTenantHandler>();

        // Event Handlers (these work with Mediator for domain events)
        services.AddScoped<TenantCreatedEventHandler>();
        services.AddScoped<INotificationHandler<TenantCreatedEvent>>(
            provider => provider.GetRequiredService<TenantCreatedEventHandler>());

        return services;
    }
}