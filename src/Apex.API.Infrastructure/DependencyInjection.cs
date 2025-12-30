using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Infrastructure.Data;
using Apex.API.Infrastructure.Identity;
using Apex.API.Infrastructure.Services;
using Apex.API.UseCases.Common.Interfaces;

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
        services.AddScoped<IDomainEventDispatcher, Apex.API.Infrastructure.Services.MediatorDomainEventDispatcher>();

        // Services
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

        // ========================================================================
        // MEDIATR: Auto-discovers all handlers! ✨
        // ========================================================================
        // This will find:
        // - All IRequestHandler<TRequest, TResponse> implementations
        // - All INotificationHandler<TNotification> implementations
        // From the UseCases assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Apex.API.UseCases.Tenants.Create.CreateTenantHandler).Assembly);
        });

        return services;
    }
}
