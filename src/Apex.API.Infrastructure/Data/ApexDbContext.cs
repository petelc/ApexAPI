using Microsoft.EntityFrameworkCore;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.Infrastructure.Data;

/// <summary>
/// Main database context for APEX
/// TEMPORARY: Domain events disabled until Mediator is properly configured
/// </summary>
public class ApexDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private ITenantContext? _tenantContext;

    // DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public ApexDbContext(
        DbContextOptions<ApexDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public void SetTenantContext(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApexDbContext).Assembly);
        modelBuilder.HasDefaultSchema("shared");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get entities with domain events BEFORE saving
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // TEMPORARILY DISABLED: Domain event dispatching
        // TODO: Re-enable once Mediator is properly configured with event handlers
        /*
        if (entitiesWithEvents.Any())
        {
            await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }
        */

        // For now, just clear the events without dispatching
        if (entitiesWithEvents.Any())
        {
            foreach (var entity in entitiesWithEvents)
            {
                entity.ClearDomainEvents();
            }
        }

        return result;
    }
}