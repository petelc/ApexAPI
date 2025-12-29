using Microsoft.EntityFrameworkCore;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TenantAggregate;

namespace Apex.API.Infrastructure.Data;

/// <summary>
/// Main database context for APEX
/// NOW WITH DOMAIN EVENTS ENABLED! ✅
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

        // Save changes to database first
        var result = await base.SaveChangesAsync(cancellationToken);

        // ✅ DOMAIN EVENTS NOW ENABLED!
        // Dispatch events AFTER successful save
        if (entitiesWithEvents.Any())
        {
            await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }

        return result;
    }
}
