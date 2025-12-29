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
using Apex.API.Core.ValueObjects;
using Mediator;
using Ardalis.Result;

namespace Apex.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Simple DbContext registration
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

        // Required services
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

        // ===================================================================
        // MEDIATOR: Register Command Handlers
        // ===================================================================
        services.AddScoped<IRequestHandler<CreateTenantCommand, Result<TenantId>>, CreateTenantHandler>();
        
        // Add more command handlers here as you create them:
        // services.AddScoped<IRequestHandler<UpdateTenantCommand, Result>, UpdateTenantHandler>();
        // services.AddScoped<IRequestHandler<DeleteTenantCommand, Result>, DeleteTenantHandler>();

        // ===================================================================
        // MEDIATOR: Register Event Handlers (Domain Events)
        // ===================================================================
        services.AddScoped<INotificationHandler<TenantCreatedEvent>, TenantCreatedEventHandler>();
        
        // Add more event handlers here as you create them:
        // services.AddScoped<INotificationHandler<TenantStatusChangedEvent>, TenantStatusChangedEventHandler>();
        // services.AddScoped<INotificationHandler<TenantTierUpgradedEvent>, TenantTierUpgradedEventHandler>();

        return services;
    }
}
